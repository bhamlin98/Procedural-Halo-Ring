# Quadtree-Based LOD System Guide

## Overview

This guide explains the new Quadtree-based Level of Detail (LOD) system for the Procedural Halo Ring project. This system provides adaptive terrain detail based on player proximity, improving both visual quality and performance.

## Features

### 1. Hierarchical Chunk Management
- **Quadtree Structure**: Terrain is organized in a hierarchical tree where each node can subdivide into 4 children
- **Dynamic Subdivision**: Chunks automatically split when the player gets close
- **Automatic Merging**: Distant chunks merge back to reduce memory and rendering overhead
- **Configurable Depth**: Control maximum subdivision levels (0-6)

### 2. Terrain Erosion
Two erosion algorithms enhance terrain realism:

#### Hydraulic Erosion
Simulates water flow and sediment transport:
- Water droplets flow downhill
- Erode material from high areas
- Deposit sediment in valleys
- Creates realistic river beds and valleys

#### Thermal Erosion
Simulates talus (loose rock) sliding:
- Material slides down steep slopes
- Smooths sharp peaks
- Creates more natural-looking mountains

### 3. Debug Visualization
- **Color-Coded LOD Levels**: Green (low detail) to Red (high detail)
- **Chunk Boundaries**: Wireframe boxes show quadtree structure
- **Toggle in Inspector**: Easy on/off control

## How to Use

### Basic Setup

1. **Select your RingWorldGenerator** in the scene hierarchy
2. **Enable Quadtree LOD**:
   - Check "Use Quadtree LOD" in the inspector
3. **Assign a Player**:
   - Drag your player GameObject to the "Player" field
4. **Configure LOD Settings**:
   - **Max Quadtree Depth**: How many subdivision levels (recommended: 3-4)
   - **Subdivision Distance**: Distance at which chunks split (recommended: 500m)
   - **Merge Distance**: Distance at which chunks merge (recommended: 1000m, must be > subdivision)
   - **LOD Update Interval**: How often to check LOD (recommended: 0.2s)

### Enabling Erosion

#### Hydraulic Erosion Settings:
- **Enabled**: Check to activate
- **Iterations**: Number of water droplets (50,000 - 500,000 for good results)
- **Erosion Speed**: How fast terrain erodes (0.3 recommended)
- **Deposit Speed**: How fast sediment deposits (0.3 recommended)
- **Sediment Capacity Factor**: Controls erosion strength (4.0 recommended)

#### Thermal Erosion Settings:
- **Enabled**: Check to activate
- **Iterations**: Number of smoothing passes (50-100 recommended)
- **Talus Angle**: Slope threshold for material sliding (0.1 recommended)
- **Erosion Rate**: Speed of material transfer (0.5 recommended)

### Debug Visualization

1. **Enable Debug Bounds** in the inspector
2. **Open Scene View** (not Game view)
3. You'll see:
   - Wireframe boxes around each chunk
   - Color-coded LOD levels (green = coarse, red = fine detail)

## Performance Tuning

### For Better Performance:
- Reduce `maxQuadtreeDepth` (try 2-3)
- Increase `subdivisionDistance` (try 800-1000m)
- Decrease erosion iterations
- Increase `lodUpdateInterval` (try 0.5s)

### For Better Quality:
- Increase `maxQuadtreeDepth` (try 4-5)
- Decrease `subdivisionDistance` (try 300-400m)
- Increase erosion iterations
- Decrease `lodUpdateInterval` (try 0.1s)

## Technical Details

### Quadtree Structure

```
Root Chunk
├── Child 0 (Bottom-Left)
│   ├── Grandchild 0
│   ├── Grandchild 1
│   ├── Grandchild 2
│   └── Grandchild 3
├── Child 1 (Bottom-Right)
├── Child 2 (Top-Left)
└── Child 3 (Top-Right)
```

Each chunk can subdivide into 4 equal-sized children. The maximum depth controls how many levels of subdivision are possible.

### LOD Update Cycle

1. Every `lodUpdateInterval` seconds:
2. Check player position
3. For each quadtree root:
   - If leaf and player is close: **Subdivide**
   - If has children and all are far: **Merge**
   - Recursively update all children

### Erosion Pipeline

1. Generate base noise map (Perlin noise with octaves)
2. Apply thermal erosion (if enabled) - smooths peaks
3. Apply hydraulic erosion (if enabled) - creates valleys
4. Use eroded heightmap for mesh generation

## Compatibility

### Backward Compatibility
- Set `useQuadtreeLOD = false` to use the original grid-based system
- All existing features (materials, object spawning, regions) work with both systems

### Existing Features Preserved
- ✅ Region-based materials and textures
- ✅ Object spawning on terrain
- ✅ Custom height curves
- ✅ Noise-based terrain generation
- ✅ UV mapping and texture scaling

## Troubleshooting

### Issue: Chunks flickering
**Solution**: Increase the gap between subdivision and merge distances. Merge should be at least 2x subdivision distance.

### Issue: Poor performance
**Solution**: 
- Reduce `maxQuadtreeDepth`
- Increase `lodUpdateInterval`
- Disable erosion or reduce iterations

### Issue: Not enough detail near player
**Solution**:
- Increase `maxQuadtreeDepth`
- Decrease `subdivisionDistance`
- Increase `segmentXVertices` and `segmentYVertices`

### Issue: Erosion not visible
**Solution**:
- Increase erosion iterations
- Increase erosion strength parameters
- Make sure erosion is enabled
- Check that `heightMultiplier` is not too small

## Examples

### Conservative Settings (Best Performance)
```
useQuadtreeLOD: true
maxQuadtreeDepth: 2
subdivisionDistance: 1000
mergeDistance: 2000
lodUpdateInterval: 0.5

hydraulicErosion.enabled: false
thermalErosion.enabled: false
```

### Balanced Settings (Recommended)
```
useQuadtreeLOD: true
maxQuadtreeDepth: 3
subdivisionDistance: 500
mergeDistance: 1000
lodUpdateInterval: 0.2

hydraulicErosion.enabled: true
hydraulicErosion.iterations: 50000

thermalErosion.enabled: true
thermalErosion.iterations: 50
```

### High Quality Settings (Best Visuals)
```
useQuadtreeLOD: true
maxQuadtreeDepth: 4
subdivisionDistance: 300
mergeDistance: 800
lodUpdateInterval: 0.1

hydraulicErosion.enabled: true
hydraulicErosion.iterations: 200000

thermalErosion.enabled: true
thermalErosion.iterations: 100
```

## Testing

Unit tests are provided in `Assets/Tests/Editor/`:
- `TerrainQuadtreeTests.cs`: Tests quadtree subdivision, merging, and LOD logic
- `TerrainErosionTests.cs`: Tests hydraulic and thermal erosion algorithms

Run tests via Unity Test Runner (Window > General > Test Runner).
