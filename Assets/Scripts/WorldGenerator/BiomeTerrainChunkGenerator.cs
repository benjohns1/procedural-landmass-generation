using NoiseGenerator;
using UnityEngine;
using Utilities;

namespace WorldGenerator
{
    public class BiomeTerrainChunkGenerator
    {
        private const float biomeMaskLowerThreshold = 0.0001f;
        private const float blurDeviationLowerThreshold = 0.0001f;

        public class ChunkData
        {
            public Region heightMap;
            public Material material;
        }

        public static ChunkData GenerateChunkData(Vector2 startPoint, WorldSettings worldSettings)
        {
            int width = worldSettings.meshSettings.numVertsPerLine;
            int height = worldSettings.meshSettings.numVertsPerLine;
            int extraBorderSize;
            Region biomeMap = GetBiomeMap(startPoint, worldSettings, width, height, out extraBorderSize);
            Region worldHeightMap = worldSettings.globalHeightMapSettings == null ? new Region(new float[width, height], 0, 0) : RegionGenerator.GenerateRegion(width, height, worldSettings.globalHeightMapSettings, startPoint);

            float[,] combinedHeightMap = new float[width, height];
            MinMaxFloat minMax = new MinMaxFloat();
            Material material = null;
            float worldEdgeSmoothing = worldSettings.globalBiomeSettings.heightMapEdgeSmoothing;
            foreach (BiomeSettings biome in worldSettings.biomes)
            {
                // @TODO: apply masking to materials/texturing, too (one material/shader for entire world?)
                if (material == null)
                {
                    material = biome.TerrainMaterial;
                }

                Region heightMap = RegionGenerator.GenerateRegion(width, height, biome.heightSettings, startPoint);
                float[,] blurredMask = CalculateBlurredBiomeMask(biomeMap.values, biome.worldMapBiomeId, worldEdgeSmoothing * biome.heightMapEdgeSmoothingModifier);

                minMax.AddValue(heightMap.minValue + worldHeightMap.minValue);
                minMax.AddValue(heightMap.maxValue + worldHeightMap.maxValue);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float blurredMaskValue = blurredMask[x + extraBorderSize, y + extraBorderSize];
                        float biomePointHeight = (blurredMaskValue > biomeMaskLowerThreshold) ? (heightMap.values[x, y] * blurredMaskValue) : 0;
                        combinedHeightMap[x, y] += biomePointHeight + worldHeightMap.values[x, y];
                    }
                }
            }

            return new ChunkData
            {
                heightMap = new Region(combinedHeightMap, minMax.Min, minMax.Max),
                material = material
            };
        }

        private static Region GetBiomeMap(Vector2 startPoint, WorldSettings worldSettings, int width, int height, out int extraBorderSize)
        {
            // Based on max edge-smoothing kernel width, determine how many extra points we need to grab outside of this area to calculate smoothing correctly at edge of chunk
            float worldSmoothing = worldSettings.globalBiomeSettings.heightMapEdgeSmoothing;
            float maxSmoothing = worldSmoothing;
            foreach (BiomeSettings biome in worldSettings.biomes)
            {
                float biomeSmoothing = worldSmoothing * biome.heightMapEdgeSmoothingModifier;
                if (biomeSmoothing > maxSmoothing)
                {
                    maxSmoothing = biomeSmoothing;
                }
            }
            float[] maxBlurKernel = StandardDeviation.GetKernel(maxSmoothing);
            extraBorderSize = maxBlurKernel.Length / 2;

            return worldSettings.biomeMapSettings == null ? new Region(new float[width, height], 0, 0) : RegionGenerator.GenerateRegion(width + (2 * extraBorderSize), height + (2 * extraBorderSize), worldSettings.biomeMapSettings, new Vector2(startPoint.x - extraBorderSize, startPoint.y - extraBorderSize));
        }

        private static float[,] CalculateBlurredBiomeMask(float[,] biomeMapValues, float biomeId, float deviation)
        {
            float[,] biomeMask = Matrix.ValueFilter(biomeMapValues, (val, x, y) => Mathf.Approximately(val, biomeId) ? 1f : 0f);
            if (deviation < blurDeviationLowerThreshold)
            {
                return biomeMask;
            }
            float[] blurKernel = StandardDeviation.GetKernel(deviation);
            float[,] blurredMask = Convolution.Symmetric(biomeMask, blurKernel);
            // @TODO: shrink/trim mask to actual region size
            return blurredMask;
        }
    }
}