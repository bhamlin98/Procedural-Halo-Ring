using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for RingWorldGenerator to provide better UI organization
/// </summary>
[CustomEditor(typeof(RingWorldGenerator))]
public class RingWorldGeneratorEditor : Editor
{
    private bool showErosionSettings = false;
    private bool showQuadtreeSettings = false;
    private bool showDebugSettings = false;
    
    public override void OnInspectorGUI()
    {
        RingWorldGenerator generator = (RingWorldGenerator)target;
        
        // Draw default inspector for most properties
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        // Erosion Settings Foldout
        showErosionSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showErosionSettings, "Erosion Settings Details");
        if (showErosionSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.HelpBox("Erosion is applied after noise generation. Enable to create more realistic terrain features like valleys and ridges.", MessageType.Info);
            
            EditorGUILayout.LabelField("Hydraulic Erosion", EditorStyles.boldLabel);
            if (generator.hydraulicErosion.enabled)
            {
                EditorGUILayout.HelpBox("Simulates water flow and sediment transport. Higher iterations create more pronounced valleys.", MessageType.None);
            }
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Thermal Erosion", EditorStyles.boldLabel);
            if (generator.thermalErosion.enabled)
            {
                EditorGUILayout.HelpBox("Simulates material sliding down steep slopes. Smooths sharp peaks and ridges.", MessageType.None);
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(5);
        
        // Quadtree LOD Settings Foldout
        showQuadtreeSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showQuadtreeSettings, "Quadtree LOD Details");
        if (showQuadtreeSettings)
        {
            EditorGUI.indentLevel++;
            
            if (generator.useQuadtreeLOD)
            {
                EditorGUILayout.HelpBox("Quadtree LOD dynamically subdivides chunks near the player for higher detail and merges distant chunks for better performance.", MessageType.Info);
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.LabelField("LOD Distances", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    $"Subdivision: {generator.subdivisionDistance}m\n" +
                    $"Merge: {generator.mergeDistance}m\n" +
                    $"Max Depth: {generator.maxQuadtreeDepth} levels\n" +
                    $"Update Interval: {generator.lodUpdateInterval}s",
                    MessageType.None
                );
                
                if (generator.mergeDistance <= generator.subdivisionDistance)
                {
                    EditorGUILayout.HelpBox("Warning: Merge distance should be larger than subdivision distance to prevent flickering!", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Quadtree LOD is disabled. Using traditional grid-based chunk system.", MessageType.Info);
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(5);
        
        // Debug Settings Foldout
        showDebugSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showDebugSettings, "Debug Visualization");
        if (showDebugSettings)
        {
            EditorGUI.indentLevel++;
            
            if (generator.showDebugBounds && generator.useQuadtreeLOD)
            {
                EditorGUILayout.HelpBox("Debug bounds will be visible in the Scene view. Green = low detail, Red = high detail.", MessageType.Info);
            }
            else if (generator.showDebugBounds && !generator.useQuadtreeLOD)
            {
                EditorGUILayout.HelpBox("Debug bounds only work with Quadtree LOD enabled.", MessageType.Warning);
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(10);
        
        // Generate Button
        if (GUILayout.Button("Generate Halo Ring", GUILayout.Height(30)))
        {
            generator.Generate();
        }
        
        // Save changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
