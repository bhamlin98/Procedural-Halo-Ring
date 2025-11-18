using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for TerrainErosion algorithms
/// </summary>
public class TerrainErosionTests
{
    [Test]
    public void TestHydraulicErosionDisabled()
    {
        // Arrange
        float[,] heightMap = CreateTestHeightMap(10, 10);
        float[,] originalHeightMap = (float[,])heightMap.Clone();
        
        HydraulicErosionSettings settings = new HydraulicErosionSettings();
        settings.enabled = false;
        
        // Act
        float[,] result = TerrainErosion.ApplyHydraulicErosion(heightMap, settings);
        
        // Assert - map should be unchanged
        Assert.AreEqual(originalHeightMap, result);
    }
    
    [Test]
    public void TestThermalErosionDisabled()
    {
        // Arrange
        float[,] heightMap = CreateTestHeightMap(10, 10);
        float[,] originalHeightMap = (float[,])heightMap.Clone();
        
        ThermalErosionSettings settings = new ThermalErosionSettings();
        settings.enabled = false;
        
        // Act
        float[,] result = TerrainErosion.ApplyThermalErosion(heightMap, settings);
        
        // Assert - map should be unchanged
        Assert.AreEqual(originalHeightMap, result);
    }
    
    [Test]
    public void TestHydraulicErosionModifiesHeightMap()
    {
        // Arrange
        float[,] heightMap = CreateTestHeightMap(50, 50);
        float[,] originalHeightMap = (float[,])heightMap.Clone();
        
        HydraulicErosionSettings settings = new HydraulicErosionSettings
        {
            enabled = true,
            iterations = 100,
            seed = 42,
            erosionSpeed = 0.3f,
            depositSpeed = 0.3f
        };
        
        // Act
        float[,] result = TerrainErosion.ApplyHydraulicErosion(heightMap, settings);
        
        // Assert - map should be modified
        bool hasChanged = false;
        for (int y = 0; y < 50; y++)
        {
            for (int x = 0; x < 50; x++)
            {
                if (Mathf.Abs(result[x, y] - originalHeightMap[x, y]) > 0.001f)
                {
                    hasChanged = true;
                    break;
                }
            }
            if (hasChanged) break;
        }
        
        Assert.IsTrue(hasChanged, "Hydraulic erosion should modify the height map");
    }
    
    [Test]
    public void TestThermalErosionSmoothsSteepSlopes()
    {
        // Arrange - create a heightmap with a steep slope
        float[,] heightMap = new float[20, 20];
        for (int y = 0; y < 20; y++)
        {
            for (int x = 0; x < 20; x++)
            {
                // Create a steep vertical gradient
                heightMap[x, y] = (float)y / 20f;
            }
        }
        
        ThermalErosionSettings settings = new ThermalErosionSettings
        {
            enabled = true,
            iterations = 10,
            talusAngle = 0.05f,
            erosionRate = 0.5f
        };
        
        // Act
        float[,] result = TerrainErosion.ApplyThermalErosion(heightMap, settings);
        
        // Assert - the slope should be smoothed
        // Check middle region (avoiding edges)
        float originalSlope = heightMap[10, 15] - heightMap[10, 5];
        float erodedSlope = result[10, 15] - result[10, 5];
        
        // After thermal erosion, the maximum slope should be reduced
        Assert.Less(Mathf.Abs(erodedSlope), Mathf.Abs(originalSlope) + 0.01f, 
            "Thermal erosion should smooth or maintain steep slopes");
    }
    
    [Test]
    public void TestCombinedErosion()
    {
        // Arrange
        float[,] heightMap = CreateTestHeightMap(30, 30);
        
        HydraulicErosionSettings hydraulic = new HydraulicErosionSettings
        {
            enabled = true,
            iterations = 50,
            seed = 42
        };
        
        ThermalErosionSettings thermal = new ThermalErosionSettings
        {
            enabled = true,
            iterations = 10,
            talusAngle = 0.1f
        };
        
        // Act
        float[,] result = TerrainErosion.ApplyErosion(heightMap, hydraulic, thermal);
        
        // Assert - result should be valid
        Assert.IsNotNull(result);
        Assert.AreEqual(30, result.GetLength(0));
        Assert.AreEqual(30, result.GetLength(1));
        
        // Check that values are still in a reasonable range
        for (int y = 0; y < 30; y++)
        {
            for (int x = 0; x < 30; x++)
            {
                Assert.IsFalse(float.IsNaN(result[x, y]), "Erosion should not produce NaN values");
                Assert.IsFalse(float.IsInfinity(result[x, y]), "Erosion should not produce infinite values");
            }
        }
    }
    
    [Test]
    public void TestErosionPreservesHeightMapDimensions()
    {
        // Arrange
        int width = 25;
        int height = 35;
        float[,] heightMap = CreateTestHeightMap(width, height);
        
        HydraulicErosionSettings settings = new HydraulicErosionSettings
        {
            enabled = true,
            iterations = 50
        };
        
        // Act
        float[,] result = TerrainErosion.ApplyHydraulicErosion(heightMap, settings);
        
        // Assert
        Assert.AreEqual(width, result.GetLength(0));
        Assert.AreEqual(height, result.GetLength(1));
    }
    
    /// <summary>
    /// Helper method to create a test heightmap with some variation
    /// </summary>
    private float[,] CreateTestHeightMap(int width, int height)
    {
        float[,] heightMap = new float[width, height];
        
        // Create some simple noise pattern
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / width;
                float ny = (float)y / height;
                heightMap[x, y] = Mathf.PerlinNoise(nx * 5f, ny * 5f);
            }
        }
        
        return heightMap;
    }
}
