using NoiseGenerator;
using UnityEngine;
using Utilities;

namespace WorldGenerator
{
    [CreateAssetMenu()]
    public class WorldSettings : UpdatableSettings
    {
        public NoiseSettings worldHeightMap;
        public BiomeSettings[] biomes;
        public Gradient biomeCellGradient;

        [HideInInspector]
        public NoiseSettings biomeMap;

        public WorldBiomeSettings worldBiomeSettings;

        [System.Serializable]
        public struct WorldBiomeSettings
        {
            public int seed;
            public float scale;
            public float frequency;
            public FastNoise.CellularDistanceFunction distanceFunction;
        }

        public WorldSettings()
        {
            Initialize();
        }

        public void Initialize()
        {
            float totalFrequency = 0;
            foreach (BiomeSettings biome in biomes)
            {
                totalFrequency += biome.frequency;
            }
            biomeMap = GenerateBiomeMapSettings(biomes, worldBiomeSettings, totalFrequency);
        }

        private static NoiseSettings GenerateBiomeMapSettings(BiomeSettings[] biomes, WorldBiomeSettings worldBiomeSettings, float totalFrequency)
        {
            return new NoiseSettings
            {
                noiseLayers = new NoiseLayer[]
                {
                    new NoiseLayer
                    {
                        enabled = true,
                        layerOperator = NoiseLayer.LayerOperator.Set,
                        settings = new FilterSettings
                        {
                            cellularSettings = new FilterSettings.CellularStep
                            {
                                seed = worldBiomeSettings.seed,
                                scale = worldBiomeSettings.scale,
                                frequency = worldBiomeSettings.frequency,
                                distanceFunction = worldBiomeSettings.distanceFunction,
                                cellTypes = new Gradient
                                {
                                    mode = GradientMode.Fixed,
                                    colorKeys = CalculateBiomeGradientKeys(biomes, totalFrequency)
                                }
                            }
                        }
                    }
                }
            };
        }

        private static GradientColorKey[] CalculateBiomeGradientKeys(BiomeSettings[] biomes, float totalFrequency)
        {
            int numberOfBiomes = biomes.Length;
            if (numberOfBiomes <= 0)
            {
                return new GradientColorKey[0];
            }

            GradientColorKey[] biomeKeys = new GradientColorKey[numberOfBiomes];
            float colorDivisor = Mathf.Max(numberOfBiomes - 1, 1);
            for (int i = 0; i < numberOfBiomes; i++)
            {
                float time = biomes[i].frequency / totalFrequency;
                float colorId = (float)i / colorDivisor;
                biomes[i].worldMapBiomeId = colorId;
                biomeKeys[i] = new GradientColorKey
                {
                    time = time,
                    color = new Color(colorId, colorId, colorId)
                };
            }
            return biomeKeys;
        }
    }
}
