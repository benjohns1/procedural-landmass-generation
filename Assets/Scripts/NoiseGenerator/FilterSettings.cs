using UnityEngine;
using Utilities;

namespace NoiseGenerator
{
    [System.Serializable]
    public class FilterSettings
    {
        public enum FilterType { Perlin, Constant, Cellular };
        public FilterType filterType;

        [ConditionalHide("filterType", 0)]
        public Perlin perlinSettings;

        [ConditionalHide("filterType", 1)]
        public Constant constantSettings;

        [ConditionalHide("filterType", 2)]
        public Cellular cellularSettings;

        [System.Serializable]
        public class Perlin
        {
            public float scale = 50;
            public float multiplier = 1;

            public int octaves = 6;
            [Range(0, 1)]
            public float persistence = 0.6f;
            public float lacunarity = 2;

            public int seed;
            public Vector2 offset;

            public void Validate()
            {
                scale = Mathf.Max(scale, 0.0001f);
                octaves = Mathf.Max(octaves, 1);
                lacunarity = Mathf.Max(lacunarity, 1);
                persistence = Mathf.Clamp01(persistence);
            }
        }

        [System.Serializable]
        public class Constant
        {
            public float value;
        }

        [System.Serializable]
        public class Cellular
        {
            public int seed;
            public Gradient cellTypes;

            public void Validate()
            {
                cellTypes.mode = GradientMode.Fixed;
            }
        }

        public void Validate()
        {
            if (perlinSettings != null)
            {
                perlinSettings.Validate();
            }
        }
    }
}