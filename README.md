# Procedural-Halo-Ring
This project procedurally generates a 1/100x scale Halo ring using Perlin Noise and Fractional Brownian Motion to simulate realistic terrain features.

**[Check out the demo here!](https://97saundersj.github.io/Procedural-Halo-Ring/)**

## Features
- Procedural generation of a Halo ring with customizable parameters.
- Realistic terrain generation using Perlin Noise and fBm.
- **NEW: Quadtree-based adaptive Level of Detail (LOD)** for dynamic terrain detail based on player proximity.
- **NEW: Hydraulic and Thermal Erosion algorithms** for enhanced terrain realism with valleys, ridges, and natural features.
- Adjustable rendering options for the interior and exterior of the ring.
- Dynamic mesh generation and texture application.
- Region-based materials and object spawning.

## New in This Update

### Quadtree-Based LOD System
- **Adaptive Detail**: Terrain chunks dynamically subdivide near the player for higher detail and merge when far away
- **Performance Optimized**: Only render high-detail meshes where needed
- **Configurable**: Control subdivision depth, distances, and update frequency
- **Debug Visualization**: Color-coded LOD levels visible in Scene view

### Advanced Terrain Erosion
- **Hydraulic Erosion**: Simulates water flow and sediment transport to create realistic valleys and riverbeds
- **Thermal Erosion**: Simulates material sliding down slopes to create natural-looking mountains
- **Fully Configurable**: Adjust erosion intensity, iterations, and behavior
- **Optional**: Can be disabled for original terrain appearance

### Testing & Documentation
- Comprehensive unit tests for quadtree and erosion systems
- Detailed [Quadtree LOD Guide](QUADTREE_LOD_GUIDE.md) with usage examples
- Custom inspector UI for easier configuration

See [QUADTREE_LOD_GUIDE.md](QUADTREE_LOD_GUIDE.md) for detailed usage instructions.

## Screenshots
Procedurally Generated Interior
![Halo paininting](https://github.com/user-attachments/assets/545d7cb5-75fe-4006-ac36-8f69f0552f12)


Exterior with custom textures
![image](https://github.com/user-attachments/assets/7a5e00f0-66ef-4880-a3ae-f9da7d776d72)
