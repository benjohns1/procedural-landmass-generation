using UnityEngine;
using NoiseGenerator;
using Utilities;

namespace WorldGenerator
{
    [CreateAssetMenu()]
    public class BiomeSettings : UpdatableSettings
    {
        public NoiseSettings heightSettings;
        public TextureSettings textureSettings;
        public float heightMapEdgeSmoothingModifier = 1f;
        public float frequency = 1f;

        public Material TerrainMaterial { get; private set; }
        [HideInInspector]
        public float worldMapBiomeValue;

        public void Initialize(int width, int height, Material baseMaterial)
        {
            Region dummyheightMap = RegionGenerator.GenerateRegion(width, height, heightSettings, Vector2.zero, true);
            TerrainMaterial = Instantiate<Material>(baseMaterial);
            textureSettings.ApplyToMaterial(TerrainMaterial);
            textureSettings.UpdateMeshHeights(TerrainMaterial, dummyheightMap.minValue, dummyheightMap.maxValue);
        }
    }
}