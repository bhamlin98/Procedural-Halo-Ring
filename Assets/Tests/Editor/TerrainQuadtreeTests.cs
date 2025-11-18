using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Unit tests for TerrainQuadtreeChunk functionality
/// </summary>
public class TerrainQuadtreeTests
{
    private GameObject testGeneratorObj;
    private RingWorldGenerator testGenerator;
    
    [SetUp]
    public void Setup()
    {
        // Create a test generator
        testGeneratorObj = new GameObject("TestGenerator");
        testGenerator = testGeneratorObj.AddComponent<RingWorldGenerator>();
        
        // Set up minimal required properties
        testGenerator.radiusInMeters = 50000f;
        testGenerator.widthInMeters = 3000;
        testGenerator.NumberOfCircumferenceChunks = 16;
        testGenerator.NumberOfWidthChunks = 4;
        testGenerator.segmentXVertices = 16;
        testGenerator.segmentYVertices = 2;
        testGenerator.maxMeshLevelOfDetail = 3;
        testGenerator.levelOfDetail = 0;
        testGenerator.noiseScale = 50f;
        testGenerator.octaves = 4;
        testGenerator.persistance = 0.5f;
        testGenerator.lacunarity = 2f;
        testGenerator.seed = 42;
        testGenerator.heightMultiplier = 10f;
        testGenerator.meshHeightMultiplier = 100f;
        
        // Initialize required arrays
        testGenerator.regions = new TerrainType[0];
    }
    
    [TearDown]
    public void Teardown()
    {
        if (testGeneratorObj != null)
        {
            Object.DestroyImmediate(testGeneratorObj);
        }
    }
    
    [Test]
    public void TestQuadtreeInitialization()
    {
        // Arrange
        GameObject rootObj = new GameObject("TestRoot");
        TerrainQuadtreeChunk chunk = rootObj.AddComponent<TerrainQuadtreeChunk>();
        
        // Act
        chunk.Initialize(testGenerator, null, 0, 3, 0, 1, 0, 1, 16, 4, 0, 0);
        
        // Assert
        Assert.AreEqual(0, chunk.depth);
        Assert.AreEqual(3, chunk.maxDepth);
        Assert.IsTrue(chunk.isLeaf);
        Assert.IsNull(chunk.children);
        Assert.AreEqual(testGenerator, chunk.generator);
        
        // Cleanup
        Object.DestroyImmediate(rootObj);
    }
    
    [Test]
    public void TestQuadtreeSubdivision()
    {
        // Arrange
        GameObject rootObj = new GameObject("TestRoot");
        TerrainQuadtreeChunk chunk = rootObj.AddComponent<TerrainQuadtreeChunk>();
        chunk.Initialize(testGenerator, null, 0, 3, 0, 2, 0, 2, 16, 4, 0, 0);
        
        // Act
        chunk.Subdivide();
        
        // Assert
        Assert.IsFalse(chunk.isLeaf);
        Assert.IsNotNull(chunk.children);
        Assert.AreEqual(4, chunk.children.Length);
        
        // Check that all children were created
        foreach (var child in chunk.children)
        {
            Assert.IsNotNull(child);
            Assert.AreEqual(1, child.depth);
            Assert.IsTrue(child.isLeaf);
        }
        
        // Cleanup
        Object.DestroyImmediate(rootObj);
    }
    
    [Test]
    public void TestQuadtreeMaxDepthPreventsSubdivision()
    {
        // Arrange
        GameObject rootObj = new GameObject("TestRoot");
        TerrainQuadtreeChunk chunk = rootObj.AddComponent<TerrainQuadtreeChunk>();
        chunk.Initialize(testGenerator, null, 3, 3, 0, 1, 0, 1, 16, 4, 0, 0);
        
        // Act
        chunk.Subdivide();
        
        // Assert - should still be a leaf since we're at max depth
        Assert.IsTrue(chunk.isLeaf);
        Assert.IsNull(chunk.children);
        
        // Cleanup
        Object.DestroyImmediate(rootObj);
    }
    
    [Test]
    public void TestQuadtreeMerge()
    {
        // Arrange
        GameObject rootObj = new GameObject("TestRoot");
        TerrainQuadtreeChunk chunk = rootObj.AddComponent<TerrainQuadtreeChunk>();
        chunk.Initialize(testGenerator, null, 0, 3, 0, 2, 0, 2, 16, 4, 0, 0);
        
        // Subdivide first
        chunk.Subdivide();
        Assert.IsFalse(chunk.isLeaf);
        
        // Act - merge back
        chunk.Merge();
        
        // Assert
        Assert.IsTrue(chunk.isLeaf);
        Assert.IsNull(chunk.children);
        
        // Cleanup
        Object.DestroyImmediate(rootObj);
    }
    
    [Test]
    public void TestShouldSubdivideBasedOnDistance()
    {
        // Arrange
        GameObject rootObj = new GameObject("TestRoot");
        rootObj.transform.position = Vector3.zero;
        
        TerrainQuadtreeChunk chunk = rootObj.AddComponent<TerrainQuadtreeChunk>();
        chunk.Initialize(testGenerator, null, 0, 3, 0, 1, 0, 1, 16, 4, 0, 0);
        
        // Create a mesh object so distance can be calculated
        chunk.chunkObject = new GameObject("ChunkMesh");
        chunk.chunkObject.transform.SetParent(rootObj.transform, false);
        var meshRenderer = chunk.chunkObject.AddComponent<MeshRenderer>();
        var meshFilter = chunk.chunkObject.AddComponent<MeshFilter>();
        
        // Create a simple mesh
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { 
            new Vector3(-100, 0, -100), 
            new Vector3(100, 0, -100), 
            new Vector3(-100, 0, 100), 
            new Vector3(100, 0, 100) 
        };
        mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
        
        // Act & Assert - close player position
        Vector3 closePosition = new Vector3(50, 0, 50);
        bool shouldSubdivideClose = chunk.ShouldSubdivide(closePosition, 500f);
        Assert.IsTrue(shouldSubdivideClose);
        
        // Act & Assert - far player position
        Vector3 farPosition = new Vector3(5000, 0, 5000);
        bool shouldSubdivideFar = chunk.ShouldSubdivide(farPosition, 500f);
        Assert.IsFalse(shouldSubdivideFar);
        
        // Cleanup
        Object.DestroyImmediate(rootObj);
    }
}
