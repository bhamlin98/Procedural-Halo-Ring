using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a node in the quadtree structure for terrain chunk management.
/// Supports recursive subdivision and merging based on player proximity for adaptive LOD.
/// </summary>
public class TerrainQuadtreeChunk : MonoBehaviour
{
    // Tree structure
    public TerrainQuadtreeChunk parent;
    public TerrainQuadtreeChunk[] children; // 4 children for quadtree
    
    // Chunk properties
    public RingWorldGenerator generator;
    public int depth; // Current depth in the tree (0 = root)
    public int maxDepth; // Maximum subdivision depth
    
    // Spatial bounds for this chunk
    public int circumferenceStartIndex;
    public int circumferenceEndIndex;
    public int widthStartIndex;
    public int widthEndIndex;
    
    // Chunk dimensions at this level
    public int numberOfCircumferenceChunks;
    public int numberOfWidthChunks;
    public int circumferenceChunkIndex;
    public int widthChunkIndex;
    
    // LOD and mesh data
    public int levelOfDetail;
    public int meshLevelOfDetail;
    public GameObject chunkObject;
    public bool isLeaf = true;
    
    // Noise and mesh generation
    public Dictionary<Vector3, float> vertexNoiseMap;
    private RingWorldChunk meshGenerator;
    
    /// <summary>
    /// Initialize a quadtree chunk node
    /// </summary>
    public void Initialize(RingWorldGenerator gen, TerrainQuadtreeChunk parentNode, int currentDepth, int maxDepth,
        int circumStart, int circumEnd, int widthStart, int widthEnd,
        int numCircumChunks, int numWidthChunks, int circumIndex, int widthIndex)
    {
        generator = gen;
        parent = parentNode;
        depth = currentDepth;
        this.maxDepth = maxDepth;
        
        circumferenceStartIndex = circumStart;
        circumferenceEndIndex = circumEnd;
        widthStartIndex = widthStart;
        widthEndIndex = widthEnd;
        
        numberOfCircumferenceChunks = numCircumChunks;
        numberOfWidthChunks = numWidthChunks;
        circumferenceChunkIndex = circumIndex;
        widthChunkIndex = widthIndex;
        
        levelOfDetail = gen.levelOfDetail;
        meshLevelOfDetail = Mathf.Max(0, gen.maxMeshLevelOfDetail - depth);
        
        isLeaf = true;
        children = null;
    }
    
    /// <summary>
    /// Generate mesh for this chunk
    /// </summary>
    public void GenerateMesh()
    {
        if (chunkObject == null)
        {
            chunkObject = new GameObject($"Chunk_D{depth}_C{circumferenceChunkIndex}_W{widthChunkIndex}");
            chunkObject.transform.SetParent(transform, false);
        }
        
        // Create or get the mesh generator component
        if (meshGenerator == null)
        {
            meshGenerator = new RingWorldChunk(
                generator,
                transform.parent != null ? transform.parent.gameObject : gameObject,
                numberOfCircumferenceChunks,
                circumferenceChunkIndex,
                numberOfWidthChunks,
                widthChunkIndex,
                levelOfDetail,
                meshLevelOfDetail
            );
        }
        
        int segmentIndexCount = generator.segmentXVertices * (generator.segmentYVertices - 1) * 6;
        meshGenerator.GenerateChunk(chunkObject, segmentIndexCount);
        
        // Store reference in generator's list
        if (!generator.createdSegments.Contains(chunkObject))
        {
            generator.createdSegments.Add(chunkObject);
        }
    }
    
    /// <summary>
    /// Subdivide this chunk into 4 children
    /// </summary>
    public void Subdivide()
    {
        if (!isLeaf || depth >= maxDepth)
        {
            return; // Already subdivided or at max depth
        }
        
        // Calculate dimensions for children
        int circumMid = (circumferenceStartIndex + circumferenceEndIndex) / 2;
        int widthMid = (widthStartIndex + widthEndIndex) / 2;
        
        // Create 4 children
        children = new TerrainQuadtreeChunk[4];
        
        // Bottom-Left
        children[0] = CreateChild(0, circumferenceStartIndex, circumMid, widthStartIndex, widthMid);
        
        // Bottom-Right
        children[1] = CreateChild(1, circumMid, circumferenceEndIndex, widthStartIndex, widthMid);
        
        // Top-Left
        children[2] = CreateChild(2, circumferenceStartIndex, circumMid, widthMid, widthEndIndex);
        
        // Top-Right
        children[3] = CreateChild(3, circumMid, circumferenceEndIndex, widthMid, widthEndIndex);
        
        isLeaf = false;
        
        // Generate meshes for children
        foreach (var child in children)
        {
            child.GenerateMesh();
        }
        
        // Destroy this chunk's mesh (children will handle rendering)
        DestroyChunkMesh();
    }
    
    /// <summary>
    /// Create a child chunk node
    /// </summary>
    private TerrainQuadtreeChunk CreateChild(int childIndex, int circumStart, int circumEnd, int widthStart, int widthEnd)
    {
        GameObject childObj = new GameObject($"QuadNode_D{depth + 1}_{childIndex}");
        childObj.transform.SetParent(transform, false);
        
        TerrainQuadtreeChunk child = childObj.AddComponent<TerrainQuadtreeChunk>();
        
        // Calculate new chunk indices for the child
        int newCircumIndex = circumferenceChunkIndex * 2 + (childIndex % 2);
        int newWidthIndex = widthChunkIndex * 2 + (childIndex / 2);
        int newNumCircumChunks = numberOfCircumferenceChunks * 2;
        int newNumWidthChunks = numberOfWidthChunks * 2;
        
        child.Initialize(
            generator,
            this,
            depth + 1,
            maxDepth,
            circumStart,
            circumEnd,
            widthStart,
            widthEnd,
            newNumCircumChunks,
            newNumWidthChunks,
            newCircumIndex,
            newWidthIndex
        );
        
        return child;
    }
    
    /// <summary>
    /// Merge children back into this chunk
    /// </summary>
    public void Merge()
    {
        if (isLeaf || children == null)
        {
            return; // Already a leaf or no children
        }
        
        // Destroy all children
        foreach (var child in children)
        {
            if (child != null)
            {
                child.DestroyRecursive();
            }
        }
        
        children = null;
        isLeaf = true;
        
        // Regenerate mesh for this chunk
        GenerateMesh();
    }
    
    /// <summary>
    /// Recursively destroy this chunk and all its children
    /// </summary>
    public void DestroyRecursive()
    {
        if (!isLeaf && children != null)
        {
            foreach (var child in children)
            {
                if (child != null)
                {
                    child.DestroyRecursive();
                }
            }
        }
        
        DestroyChunkMesh();
        
        if (Application.isEditor && !Application.isPlaying)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Destroy only the mesh of this chunk, not the node itself
    /// </summary>
    private void DestroyChunkMesh()
    {
        if (chunkObject != null)
        {
            generator.createdSegments.Remove(chunkObject);
            
            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(chunkObject);
            }
            else
            {
                Destroy(chunkObject);
            }
            
            chunkObject = null;
        }
    }
    
    /// <summary>
    /// Check if player is close enough to warrant subdivision
    /// </summary>
    public bool ShouldSubdivide(Vector3 playerPosition, float subdivisionDistance)
    {
        if (depth >= maxDepth || !isLeaf)
        {
            return false;
        }
        
        // If no mesh yet, can't check distance
        if (chunkObject == null)
        {
            return false;
        }
        
        MeshRenderer renderer = chunkObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Vector3 closestPoint = renderer.bounds.ClosestPoint(playerPosition);
            float distance = Vector3.Distance(playerPosition, closestPoint);
            return distance <= subdivisionDistance;
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if all children are far enough that this chunk should merge
    /// </summary>
    public bool ShouldMerge(Vector3 playerPosition, float mergeDistance)
    {
        if (isLeaf || children == null)
        {
            return false;
        }
        
        // Check distance to all leaf descendants
        float minDistance = GetMinDistanceToLeaves(playerPosition);
        return minDistance > mergeDistance;
    }
    
    /// <summary>
    /// Get minimum distance from player to any leaf descendant
    /// </summary>
    private float GetMinDistanceToLeaves(Vector3 playerPosition)
    {
        if (isLeaf)
        {
            if (chunkObject != null)
            {
                MeshRenderer renderer = chunkObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Vector3 closestPoint = renderer.bounds.ClosestPoint(playerPosition);
                    return Vector3.Distance(playerPosition, closestPoint);
                }
            }
            return float.MaxValue;
        }
        
        float minDist = float.MaxValue;
        foreach (var child in children)
        {
            if (child != null)
            {
                float dist = child.GetMinDistanceToLeaves(playerPosition);
                minDist = Mathf.Min(minDist, dist);
            }
        }
        
        return minDist;
    }
    
    /// <summary>
    /// Update LOD based on player position (recursive)
    /// </summary>
    public void UpdateLOD(Vector3 playerPosition, float subdivisionDistance, float mergeDistance)
    {
        if (isLeaf)
        {
            // Check if we should subdivide
            if (ShouldSubdivide(playerPosition, subdivisionDistance))
            {
                Subdivide();
            }
        }
        else
        {
            // Check if we should merge
            if (ShouldMerge(playerPosition, mergeDistance))
            {
                Merge();
            }
            else
            {
                // Recursively update children
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        if (child != null)
                        {
                            child.UpdateLOD(playerPosition, subdivisionDistance, mergeDistance);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Draw debug visualization for this chunk and its children
    /// </summary>
    public void DrawDebugGizmos()
    {
        if (isLeaf && chunkObject != null)
        {
            // Color based on depth for LOD visualization
            Color color = GetLODColor(depth);
            Gizmos.color = color;
            
            MeshRenderer renderer = chunkObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
            }
        }
        else if (!isLeaf && children != null)
        {
            // Recursively draw children
            foreach (var child in children)
            {
                if (child != null)
                {
                    child.DrawDebugGizmos();
                }
            }
        }
    }
    
    /// <summary>
    /// Get color for LOD level visualization
    /// </summary>
    private Color GetLODColor(int depth)
    {
        // Create a gradient from green (low detail) to red (high detail)
        float t = (float)depth / maxDepth;
        return Color.Lerp(Color.green, Color.red, t);
    }
}
