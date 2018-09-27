using NoiseGenerator;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace WorldGenerator
{
    public class BiomeTerrainChunkGenerator
    {
        private const float blurDeviationLowerThreshold = 0.0001f;

        public class ChunkData
        {
            public Region heightMap;
            public Material material;
            public int[] activeBiomeInstanceIDs;
        }

        public static ChunkData GenerateChunkData(Vector2 startPoint, Vector2 chunkCoord, WorldSettings worldSettings)
        {
            int width = worldSettings.meshSettings.numVertsPerLine;
            int height = worldSettings.meshSettings.numVertsPerLine;
            int extraBorderSize;
            Region biomeMap = GetBiomeMap(chunkCoord, worldSettings, out extraBorderSize);
            Region worldHeightMap = worldSettings.globalHeightMapSettings == null ? new Region(new float[width, height], 0, 0) : RegionGenerator.GenerateRegion(width, height, worldSettings.globalHeightMapSettings, startPoint);

            float[,] combinedHeightMap = new float[width, height];
            List<int> activeBiomes = new List<int>();
            MinMaxFloat minMax = new MinMaxFloat();
            Material material = null;
            float worldEdgeSmoothing = worldSettings.globalBiomeSettings.heightMapEdgeSmoothing;

            // Add global world height
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    combinedHeightMap[x, y] += worldHeightMap.values[x, y];
                }
            }
            string debug = "";

            // Add biome height
            foreach (BiomeSettings biome in worldSettings.biomes)
            {
                // @TODO: apply masking to materials/texturing, too (one material/shader for entire world?)
                if (material == null)
                {
                    material = biome.TerrainMaterial;
                }

                Region heightMap = RegionGenerator.GenerateRegion(width, height, biome.heightSettings, startPoint);

                if (Mathf.Approximately(biome.worldMapBiomeValue, 0f) && startPoint.x >= -100 && startPoint.x <= -100 && startPoint.y >= -150 && startPoint.y <= -50)
                {
                    debug = "chunkPosition" + startPoint + " ";
                }

                float[,] biomeBlurMask = CalculateBlurredBiomeMask(biomeMap.values, width, height, biome.worldMapBiomeValue, worldEdgeSmoothing * biome.heightMapEdgeSmoothingModifier, debug);


                minMax.AddValue(heightMap.minValue + worldHeightMap.minValue);
                minMax.AddValue(heightMap.maxValue + worldHeightMap.maxValue);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float biomeBlurValue = biomeBlurMask[x, y];
                        if (biomeBlurValue <= 0)
                        {
                            continue;
                        }
                        activeBiomes.Add(biome.GetInstanceID());
                        combinedHeightMap[x, y] += heightMap.values[x, y] * biomeBlurValue;
                    }
                }
            }

            if (debug != "")
            {
                Debug.Log(combinedHeightMap[0, 0] + " " + combinedHeightMap[0, height - 1] + " " + combinedHeightMap[width - 1, height - 1] + " " + combinedHeightMap[width - 1, 0]);
            }

            return new ChunkData
            {
                heightMap = new Region(combinedHeightMap, minMax.Min, minMax.Max),
                material = material
            };
        }

        /// <summary>
        /// Gets a 3x3 square surrounded by a border so a blur kernel can be applied correctly
        /// The center 'pixel' will represent the biome value for this whole chunk
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="worldSettings"></param>
        /// <param name="extraBorderSize"></param>
        /// <returns></returns>
        private static Region GetBiomeMap(Vector2 coord, WorldSettings worldSettings, out int extraBorderSize)
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
            extraBorderSize = maxBlurKernel.Length;// / 2;

            int biomeRegionSize = 3 + (2 * extraBorderSize);
            Vector2 biomeRegionStartPoint = new Vector2(coord.x - 1 - extraBorderSize, coord.y - 1 - extraBorderSize);

            return worldSettings.biomeMapSettings == null ? new Region(new float[biomeRegionSize, biomeRegionSize], 0, 0) : RegionGenerator.GenerateRegion(biomeRegionSize,biomeRegionSize, worldSettings.biomeMapSettings, biomeRegionStartPoint);
        }

        private static float[,] CalculateBlurredBiomeMask(float[,] biomeMapValues, int width, int height, float biomeMapValue, float deviation, string debug = "")
        {
            // Get a mask for just this biome ID
            float[,] biomeMask = Matrix.ValueFilter(biomeMapValues, (val, x, y) => Mathf.Approximately(val, biomeMapValue) ? 1f : 0f);
            if (deviation < blurDeviationLowerThreshold)
            {
                return biomeMask;
            }
            float[] blurKernel = StandardDeviation.GetKernel(deviation);
            /*float[,] gaussian = new float[,]
            {
                {0.0625f,0.125f,0.0625f},
                {0.125f,0.25f,0.125f},
                {0.0625f,0.125f,0.0625f},
            };*/
            int kernelOffset = blurKernel.Length / 2;

            // Blur the center 3x3 'pixels'
            int xCenter = biomeMask.GetLength(0) / 2;
            int xLeft = xCenter - 1;
            int xRight = xCenter + 1;
            int yCenter = biomeMask.GetLength(1) / 2;
            int yBottom = yCenter - 1;
            int yTop = yCenter + 1;
            float[,] blurredMask = Convolution.Symmetric(biomeMask, blurKernel, xLeft - kernelOffset, xRight + kernelOffset, yBottom - kernelOffset, yTop + kernelOffset);

            // From surrounding 'pixel' values, get values to build gradient from the center
            float centerVal = blurredMask[xCenter, yCenter];
            float topVal = blurredMask[xCenter, yTop];
            float bottomVal = blurredMask[xCenter, yBottom];
            float topEdgeVal = Mathf.Lerp(centerVal, topVal, 0.5f);
            float bottomEdgeVal = Mathf.Lerp(centerVal, bottomVal, 0.5f);
            float leftVal = blurredMask[xLeft, yCenter];
            float rightVal = blurredMask[xRight, yCenter];
            float leftEdgeVal = Mathf.Lerp(centerVal, leftVal, 0.5f);
            float rightEdgeVal = Mathf.Lerp(centerVal, rightVal, 0.5f);

            if (debug != "")
            {
                float val = biomeMapValues[biomeMapValues.GetLength(0) / 2, biomeMapValues.GetLength(1) / 2];
                Debug.Log(debug + "worldBiome: " + val + ", biome " + biomeMapValue + " center: " + centerVal + " top: " + topEdgeVal + " bottom: " + bottomEdgeVal);
            }

            // Generate gradient map for this chunk
            float[,] gradientMap = new float[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Outside vertices are out-of-mesh, need to reach edge values one vertex in
                    float yPct = (y - 1) / (float)(height - 3);
                    float xPct = (x - 1) / (float)(width - 3);
                    // Get gradient value for each axis
                    float yVal = Mathf.LerpUnclamped(bottomEdgeVal, topEdgeVal, yPct);
                    float xVal = Mathf.LerpUnclamped(leftEdgeVal, rightEdgeVal, xPct);

                    // @TODO: merge x/y gradients
                    // bottom left  : need 50%x, 50%y
                    // bottom center: need 0%x, 100%y
                    // bottom right : need 50%x, 50%y
                    // center left  : need 100%x, 0%y
                    // center       : need 50%x, 50%y
                    // center right : need 100%x, 0%y
                    // top left     : need 50%x, 50%y
                    // top center   : need 0%x, 100%y
                    // top right    : need 50%x, 50%y

                    // bottom left diagonal halfway : need 50%x, 50%y
                    // bottom center halfway        : need 0%x, 100%y
                    // bottom right idagonal halfway: need 50%x, 50%y
                    // center left halfway          : need 75%x, 25%y
                    // center right halfway         : need 75%x, 25%y

                    gradientMap[x, y] = xVal;
                }
            }

            return gradientMap;
        }
    }
}