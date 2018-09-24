using UnityEngine;

namespace NoiseGenerator
{
    [System.Serializable]
    public class FilterSettings
    {
        public enum FilterType { Perlin };
        public FilterType filterType;

        [ConditionalHide("filterType", 0)]
        public Perlin perlinSettings;

        [System.Serializable]
        public class Perlin
        {

            public float strength = 1;
            [Range(1, 8)]
            public int numLayers = 1;
            public float baseRoughness = 1;
            public float roughness = 2;
            public float persistence = 0.5f;
            public Vector3 center;
            public float minValue;
        }
    }
}