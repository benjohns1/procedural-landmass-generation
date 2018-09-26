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

        public static ChunkData GenerateChunkData(Vector2 coord, WorldSettings worldSettings)
        {
            // @TODO: run this via threaded job system
            int width = worldSettings.meshSettings.numVertsPerLine;
            int height = worldSettings.meshSettings.numVertsPerLine;
            Vector2 sampleCenter = coord * worldSettings.meshSettings.meshWorldSize / worldSettings.meshSettings.meshScale;
            Region biomeMap = RegionGenerator.GenerateRegion(width, height, worldSettings.biomeMapSettings, sampleCenter);

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
                minMax.AddValue(heightMap.minValue);
                minMax.AddValue(heightMap.maxValue);

                // @TODO: also apply world heightmap
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // @TODO: generate blur mask edges for smooth transitions
                        if (Mathf.Approximately(id, biomeMap.values[x, y]))
                        {
                            combinedHeightMap[x, y] += heightMap.values[x, y];
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