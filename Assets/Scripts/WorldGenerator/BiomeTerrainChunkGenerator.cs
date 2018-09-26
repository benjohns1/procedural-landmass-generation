using NoiseGenerator;
using UnityEngine;
using Utilities;

namespace WorldGenerator
{
    public class BiomeTerrainChunkGenerator
    {
        public class ChunkData
        {
            public Region heightMap;
            public Material material;
        }

        public static ChunkData GenerateChunkData(Vector2 sampleCenter, WorldSettings worldSettings)
        {
            int width = worldSettings.meshSettings.numVertsPerLine;
            int height = worldSettings.meshSettings.numVertsPerLine;
            Region biomeMap = worldSettings.biomeMapSettings == null ? new Region(new float[width, height], 0, 0) : RegionGenerator.GenerateRegion(width, height, worldSettings.biomeMapSettings, sampleCenter);
            Region worldHeightMap = worldSettings.globalHeightMapSettings == null ? new Region(new float[width, height], 0, 0) : RegionGenerator.GenerateRegion(width, height, worldSettings.globalHeightMapSettings, sampleCenter);

            float[,] combinedHeightMap = new float[width, height];
            MinMaxFloat minMax = new MinMaxFloat();
            Material material = null;
            foreach (BiomeSettings biome in worldSettings.biomes)
            {
                // @TODO: apply masking to materials/texturing, too (one material/shader for entire world?)
                if (material == null)
                {
                    material = biome.TerrainMaterial;
                }

                float id = biome.worldMapBiomeId;
                Region heightMap = RegionGenerator.GenerateRegion(width, height, biome.heightSettings, sampleCenter);
                minMax.AddValue(heightMap.minValue + worldHeightMap.minValue);
                minMax.AddValue(heightMap.maxValue + worldHeightMap.maxValue);

                // @TODO: also apply world heightmap
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // @TODO: generate blur mask edges for smooth transitions
                        if (Mathf.Approximately(id, biomeMap.values[x, y]))
                        {
                            combinedHeightMap[x, y] += heightMap.values[x, y] + worldHeightMap.values[x, y];
                        }
                    }
                }
            }

            return new ChunkData
            {
                heightMap = new Region(combinedHeightMap, minMax.Min, minMax.Max),
                material = material
            };
        }
    }
}