using UnityEngine;

/// <summary>
/// Provides hydraulic and thermal erosion algorithms for terrain heightmaps.
/// Applies post-processing to noise-generated terrain for enhanced realism.
/// </summary>
public static class TerrainErosion
{
    /// <summary>
    /// Apply hydraulic erosion to a heightmap
    /// Simulates water flow and sediment transport
    /// </summary>
    public static float[,] ApplyHydraulicErosion(float[,] heightMap, HydraulicErosionSettings settings)
    {
        if (!settings.enabled || settings.iterations <= 0)
        {
            return heightMap;
        }
        
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float[,] result = (float[,])heightMap.Clone();
        
        System.Random random = new System.Random(settings.seed);
        
        for (int iteration = 0; iteration < settings.iterations; iteration++)
        {
            // Start a droplet at a random position
            float posX = random.Next(0, width);
            float posY = random.Next(0, height);
            
            float dirX = 0;
            float dirY = 0;
            float speed = settings.initialSpeed;
            float water = settings.initialWaterVolume;
            float sediment = 0;
            
            for (int lifetime = 0; lifetime < settings.maxDropletLifetime; lifetime++)
            {
                int nodeX = (int)posX;
                int nodeY = (int)posY;
                
                // Calculate droplet's offset inside the cell
                float cellOffsetX = posX - nodeX;
                float cellOffsetY = posY - nodeY;
                
                // Calculate droplet's height and direction of flow
                HeightAndGradient heightAndGradient = CalculateHeightAndGradient(result, posX, posY);
                
                // Update the droplet's direction and position
                dirX = (dirX * settings.inertia - heightAndGradient.gradientX * (1 - settings.inertia));
                dirY = (dirY * settings.inertia - heightAndGradient.gradientY * (1 - settings.inertia));
                
                // Normalize direction
                float len = Mathf.Sqrt(dirX * dirX + dirY * dirY);
                if (len != 0)
                {
                    dirX /= len;
                    dirY /= len;
                }
                
                posX += dirX;
                posY += dirY;
                
                // Stop if droplet is out of bounds
                if (posX < 0 || posX >= width - 1 || posY < 0 || posY >= height - 1)
                {
                    break;
                }
                
                // Find the droplet's new height
                float newHeight = CalculateHeightAndGradient(result, posX, posY).height;
                float deltaHeight = newHeight - heightAndGradient.height;
                
                // Calculate sediment capacity
                float sedimentCapacity = Mathf.Max(-deltaHeight * speed * water * settings.sedimentCapacityFactor, settings.minSedimentCapacity);
                
                // Erode or deposit sediment
                if (sediment > sedimentCapacity || deltaHeight > 0)
                {
                    // Deposit sediment
                    float amountToDeposit = (deltaHeight > 0) ? Mathf.Min(deltaHeight, sediment) : (sediment - sedimentCapacity) * settings.depositSpeed;
                    sediment -= amountToDeposit;
                    
                    // Add the sediment to the four nodes of the current cell
                    result[nodeX, nodeY] += amountToDeposit * (1 - cellOffsetX) * (1 - cellOffsetY);
                    result[nodeX + 1, nodeY] += amountToDeposit * cellOffsetX * (1 - cellOffsetY);
                    result[nodeX, nodeY + 1] += amountToDeposit * (1 - cellOffsetX) * cellOffsetY;
                    result[nodeX + 1, nodeY + 1] += amountToDeposit * cellOffsetX * cellOffsetY;
                }
                else
                {
                    // Erode terrain
                    float amountToErode = Mathf.Min((sedimentCapacity - sediment) * settings.erosionSpeed, -deltaHeight);
                    
                    // Remove sediment from the four nodes of the current cell
                    result[nodeX, nodeY] -= amountToErode * (1 - cellOffsetX) * (1 - cellOffsetY);
                    result[nodeX + 1, nodeY] -= amountToErode * cellOffsetX * (1 - cellOffsetY);
                    result[nodeX, nodeY + 1] -= amountToErode * (1 - cellOffsetX) * cellOffsetY;
                    result[nodeX + 1, nodeY + 1] -= amountToErode * cellOffsetX * cellOffsetY;
                    
                    sediment += amountToErode;
                }
                
                // Update water and speed
                speed = Mathf.Sqrt(speed * speed + deltaHeight * settings.gravity);
                water *= (1 - settings.evaporationSpeed);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Apply thermal erosion to a heightmap
    /// Simulates talus (loose rock) sliding down steep slopes
    /// </summary>
    public static float[,] ApplyThermalErosion(float[,] heightMap, ThermalErosionSettings settings)
    {
        if (!settings.enabled || settings.iterations <= 0)
        {
            return heightMap;
        }
        
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float[,] result = (float[,])heightMap.Clone();
        
        for (int iteration = 0; iteration < settings.iterations; iteration++)
        {
            float[,] nextHeightMap = (float[,])result.Clone();
            
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    float currentHeight = result[x, y];
                    float maxHeightDiff = 0;
                    float totalHeightDiff = 0;
                    
                    // Check all 8 neighbors
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            
                            int nx = x + dx;
                            int ny = y + dy;
                            
                            float neighborHeight = result[nx, ny];
                            float heightDiff = currentHeight - neighborHeight;
                            
                            if (heightDiff > settings.talusAngle)
                            {
                                maxHeightDiff = Mathf.Max(maxHeightDiff, heightDiff);
                                totalHeightDiff += heightDiff;
                            }
                        }
                    }
                    
                    // If slope is too steep, move material
                    if (totalHeightDiff > 0)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (dx == 0 && dy == 0) continue;
                                
                                int nx = x + dx;
                                int ny = y + dy;
                                
                                float neighborHeight = result[nx, ny];
                                float heightDiff = currentHeight - neighborHeight;
                                
                                if (heightDiff > settings.talusAngle)
                                {
                                    // Calculate material transfer
                                    float transfer = settings.erosionRate * (heightDiff - settings.talusAngle) * (heightDiff / totalHeightDiff);
                                    
                                    nextHeightMap[x, y] -= transfer;
                                    nextHeightMap[nx, ny] += transfer;
                                }
                            }
                        }
                    }
                }
            }
            
            result = nextHeightMap;
        }
        
        return result;
    }
    
    /// <summary>
    /// Apply both hydraulic and thermal erosion in sequence
    /// </summary>
    public static float[,] ApplyErosion(float[,] heightMap, HydraulicErosionSettings hydraulic, ThermalErosionSettings thermal)
    {
        float[,] result = heightMap;
        
        // Apply thermal erosion first (removes sharp peaks)
        result = ApplyThermalErosion(result, thermal);
        
        // Then apply hydraulic erosion (creates valleys and deposits)
        result = ApplyHydraulicErosion(result, hydraulic);
        
        return result;
    }
    
    /// <summary>
    /// Calculate height and gradient at a position using bilinear interpolation
    /// </summary>
    private static HeightAndGradient CalculateHeightAndGradient(float[,] nodes, float posX, float posY)
    {
        int width = nodes.GetLength(0);
        int height = nodes.GetLength(1);
        
        int coordX = (int)posX;
        int coordY = (int)posY;
        
        // Ensure we're within bounds for interpolation
        coordX = Mathf.Clamp(coordX, 0, width - 2);
        coordY = Mathf.Clamp(coordY, 0, height - 2);
        
        float x = posX - coordX;
        float y = posY - coordY;
        
        // Heights of the four corners of the cell
        float heightNW = nodes[coordX, coordY];
        float heightNE = nodes[coordX + 1, coordY];
        float heightSW = nodes[coordX, coordY + 1];
        float heightSE = nodes[coordX + 1, coordY + 1];
        
        // Bilinear interpolation
        float heightN = heightNW * (1 - x) + heightNE * x;
        float heightS = heightSW * (1 - x) + heightSE * x;
        float height = heightN * (1 - y) + heightS * y;
        
        // Calculate gradients
        float gradientX = (heightNE - heightNW) * (1 - y) + (heightSE - heightSW) * y;
        float gradientY = (heightSW - heightNW) * (1 - x) + (heightSE - heightNE) * x;
        
        return new HeightAndGradient { height = height, gradientX = gradientX, gradientY = gradientY };
    }
    
    private struct HeightAndGradient
    {
        public float height;
        public float gradientX;
        public float gradientY;
    }
}

/// <summary>
/// Settings for hydraulic erosion simulation
/// </summary>
[System.Serializable]
public class HydraulicErosionSettings
{
    [Tooltip("Enable hydraulic erosion")]
    public bool enabled = false;
    
    [Tooltip("Random seed for droplet placement")]
    public int seed = 0;
    
    [Tooltip("Number of erosion iterations (droplets)")]
    [Range(1, 1000000)]
    public int iterations = 50000;
    
    [Tooltip("Maximum lifetime of a droplet in steps")]
    [Range(1, 100)]
    public int maxDropletLifetime = 30;
    
    [Tooltip("How much of previous direction is retained")]
    [Range(0f, 1f)]
    public float inertia = 0.05f;
    
    [Tooltip("Sediment capacity factor")]
    [Range(0f, 10f)]
    public float sedimentCapacityFactor = 4f;
    
    [Tooltip("Minimum sediment capacity")]
    [Range(0f, 1f)]
    public float minSedimentCapacity = 0.01f;
    
    [Tooltip("Erosion speed")]
    [Range(0f, 1f)]
    public float erosionSpeed = 0.3f;
    
    [Tooltip("Deposit speed")]
    [Range(0f, 1f)]
    public float depositSpeed = 0.3f;
    
    [Tooltip("Evaporation speed")]
    [Range(0f, 1f)]
    public float evaporationSpeed = 0.01f;
    
    [Tooltip("Gravity affecting droplet")]
    [Range(0f, 20f)]
    public float gravity = 4f;
    
    [Tooltip("Initial water volume")]
    [Range(0f, 10f)]
    public float initialWaterVolume = 1f;
    
    [Tooltip("Initial speed")]
    [Range(0f, 10f)]
    public float initialSpeed = 1f;
}

/// <summary>
/// Settings for thermal erosion simulation
/// </summary>
[System.Serializable]
public class ThermalErosionSettings
{
    [Tooltip("Enable thermal erosion")]
    public bool enabled = false;
    
    [Tooltip("Number of erosion iterations")]
    [Range(1, 1000)]
    public int iterations = 50;
    
    [Tooltip("Talus angle threshold (material slides if slope exceeds this)")]
    [Range(0f, 1f)]
    public float talusAngle = 0.1f;
    
    [Tooltip("Rate of material transfer")]
    [Range(0f, 1f)]
    public float erosionRate = 0.5f;
}
