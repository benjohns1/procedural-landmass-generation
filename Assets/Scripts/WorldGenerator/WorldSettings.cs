using NoiseGenerator;
using UnityEngine;
using Utilities;

namespace WorldGenerator
{
    [CreateAssetMenu()]
    public class WorldSettings : UpdatableSettings
    {
        public NoiseSettings globalHeightMap;
        public MeshSettings meshSettings;
        public BiomeSettings[] biomes;
        public Material baseMaterial;

        [HideInInspector]
        public NoiseSettings biomeMapSettings;

        public GlobalBiomeSettings globalBiomeSettings;

        [System.Serializable]
        public struct GlobalBiomeSettings
        {
            public int seed;
            public float scale;
            public float frequency;
            public FastNoise.CellularDistanceFunction distanceFunction;
        }

        public void Initialize()
        {
            float totalFrequency = 0;
            foreach (BiomeSettings biome in biomes)
            {
                biome.Initialize(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, baseMaterial);
                totalFrequency += biome.frequency;
            }
            biomeMapSettings = GenerateBiomeMapSettings(biomes, globalBiomeSettings, totalFrequency);
        }

        private static NoiseSettings GenerateBiomeMapSettings(BiomeSettings[] biomes, GlobalBiomeSettings worldBiomeSettings, float totalFrequency)
        {
            NoiseSettings biomeMapSettings = ScriptableObject.CreateInstance<NoiseSettings>();
            biomeMapSettings.noiseLayers = new NoiseLayer[]
            {
                new NoiseLayer
                {
                    enabled = true,
                    layerOperator = NoiseLayer.LayerOperator.Set,
                    settings = new FilterSettings
                    {
                        filterType = FilterSettings.FilterType.CellularStep,
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
            };
            return biomeMapSettings;
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
            float currentFrequency = 0;
            for (int i = 0; i < numberOfBiomes; i++)
            {
                currentFrequency += biomes[i].frequency;
                float time = currentFrequency / totalFrequency;
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
