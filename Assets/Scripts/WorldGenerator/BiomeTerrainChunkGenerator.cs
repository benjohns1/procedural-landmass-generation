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
            //string debug = "";

            // Add biome height
            foreach (BiomeSettings biome in worldSettings.biomes)
            {
                // @TODO: apply masking to materials/texturing, too (one material/shader for entire world?)
                if (material == null)
                {
                    material = biome.TerrainMaterial;
                }

                Region heightMap = RegionGenerator.GenerateRegion(width, height, biome.heightSettings, startPoint);

                /*
                if (Mathf.Approximately(biome.worldMapBiomeValue, 0f) && startPoint.x >= -150 && startPoint.x <= -50 && startPoint.y >= -150 && startPoint.y <= -50)
                {
                    debug = "chunkPosition" + startPoint + " ";
                }
                */

                float[,] biomeBlurMask = CalculateBlurredBiomeMask(biomeMap.values, width, height, biome.worldMapBiomeValue, worldEdgeSmoothing * biome.heightMapEdgeSmoothingModifier);//, debug);


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
                        activeBiomes.Add(biome.id);
                        combinedHeightMap[x, y] += heightMap.values[x, y] * biomeBlurValue;
                    }
                }
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

        private static float[,] CalculateBlurredBiomeMask(float[,] biomeMapValues, int width, int height, float biomeMapValue, float deviation)//, string debug = "")
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
            float topLeftCornerVal = (centerVal + blurredMask[xLeft, yCenter] + blurredMask[xLeft, yTop] + blurredMask[xCenter, yTop]) * 0.25f;
            float topRightCornerVal = (centerVal + blurredMask[xCenter, yTop] + blurredMask[xRight, yTop] + blurredMask[xRight, yCenter]) * 0.25f;
            float bottomLeftCornerVal = (centerVal + blurredMask[xCenter, yBottom] + blurredMask[xLeft, yBottom] + blurredMask[xLeft, yCenter]) * 0.25f;
            float bottomRightCornerVal = (centerVal + blurredMask[xRight, yCenter] + blurredMask[xRight, yBottom] + blurredMask[xCenter, yBottom]) * 0.25f;

            /*
            if (debug != "")
            {
                float val = biomeMapValues[biomeMapValues.GetLength(0) / 2, biomeMapValues.GetLength(1) / 2];
                Debug.Log(debug + "worldBiome: " + val + ", biome " + biomeMapValue + " center: " + centerVal + " topLeft: " + blurredMask[xLeft, yTop] + " topRight: " + blurredMask[xRight, yTop] + " bottomLeft: " + blurredMask[xLeft, yBottom] + " bottomRight: " + blurredMask[xRight, yBottom]);
            }*/

            // Generate gradient map for this chunk
            float[,] gradientMap = new float[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Outside vertices are out-of-mesh, need to reach edge values one vertex in
                    float yPct = (y - 1) / (float)(height - 3);
                    float xPct = (x - 1) / (float)(width - 3);
                    // @TODO: fix outside edge values to fix vertex normals

                    // Get gradient value for horizontal, vertical, and diagonal axes
                    float xyGradientAmount = xPct + yPct - 1f;
                    float yxGradientAmount = yPct - xPct;

                    float topLeftCornerWeight = Mathf.Max(0, yxGradientAmount);
                    float topRightCornerWeight = Mathf.Max(0, xyGradientAmount);
                    float bottomRightCornerWeight = Mathf.Max(0, -yxGradientAmount);
                    float bottomLeftCornerWeight = Mathf.Max(0, -xyGradientAmount);
                    float centerWeight = Mathf.Clamp01(1 - (topLeftCornerWeight + topRightCornerWeight + bottomRightCornerWeight + bottomLeftCornerWeight));

                    gradientMap[x, y] = (centerVal * centerWeight) + (topLeftCornerVal * topLeftCornerWeight) + (topRightCornerVal * topRightCornerWeight) + (bottomRightCornerVal * bottomRightCornerWeight) + (bottomLeftCornerVal * bottomLeftCornerWeight);
                }
            }

            return gradientMap;
        }
    }
}