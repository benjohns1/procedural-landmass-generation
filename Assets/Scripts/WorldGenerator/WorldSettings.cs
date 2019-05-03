using NoiseGenerator;
using UnityEngine;
using Utilities;

namespace WorldGenerator
{
    [CreateAssetMenu()]
    public class WorldSettings : UpdatableSettings
    {
        public NoiseSettings globalHeightMapSettings;
        public MeshSettings meshSettings;
        public BiomeSettings[] biomes;
        public Material baseMaterial;
        [SerializeField]
        private readonly float colliderGenerationDistanceThreshold = 200f;
        public float SqrColliderGenerationDistanceThreshold { get; private set; }

        [HideInInspector]
        public NoiseSettings biomeMapSettings;

        public GlobalBiomeSettings globalBiomeSettings;

#if UNITY_EDITOR
        [HideInInspector]
        public bool globalHeightMapSettingsFoldout;
        [HideInInspector]
        public bool meshSettingsFoldout;
        [HideInInspector]
        public bool biomesFoldout;
#endif

        [System.Serializable]
        public class GlobalBiomeSettings
        {
            public int seed;
            public float frequency = 0.001f;
            public FastNoise.CellularDistanceFunction distanceFunction = FastNoise.CellularDistanceFunction.Natural;
            public float heightMapEdgeSmoothing = 1f;

            public void OnValidate()
            {
                frequency = Mathf.Max(frequency, 0.0001f);
                heightMapEdgeSmoothing = Mathf.Clamp(heightMapEdgeSmoothing, 0.0001f, 3f);
            }
        }

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            globalBiomeSettings.OnValidate();
            SqrColliderGenerationDistanceThreshold = colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold;
            base.OnValidate();
        }
        #endif

        public void Initialize()
        {
            SqrColliderGenerationDistanceThreshold = colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold;
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
                biomes[i].worldMapBiomeValue = colorId;
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
