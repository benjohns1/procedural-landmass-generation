using UnityEngine;

namespace NoiseGenerator.Filters
{
    public class Perlin : IFilter
    {
        private const int perlinSampleMin = -100000;
        private const int perlinSampleMax = 100000;

        private FilterSettings.Perlin settings;
        private Vector2[] octaveOffsets;
        private float minValue;
        private float maxValue;

        public Perlin(FilterSettings.Perlin settings)
        {
            settings.Validate();
            this.settings = settings;
            CalculateSettings();
        }

        public void Setup(float globalMin, float globalMax)
        {
        }

        public void StartNewRegion(int width, int height)
        {
        }

        public float GetMin()
        {
            return minValue;
        }

        public float GetMax()
        {
            return maxValue;
        }

        private void CalculateSettings()
        {
            System.Random prng = new System.Random(settings.seed);
            Vector2[] octaveOffsets = new Vector2[settings.octaves];

            float minValue = 0;
            float maxValue = 0;
            float amplitude = 1;

            for (int i = 0; i < settings.octaves; i++)
            {
                // Generate different offsets for each octave
                float offsetX = prng.Next(perlinSampleMin, perlinSampleMax) + settings.offset.x;
                float offsetY = prng.Next(perlinSampleMin, perlinSampleMax) - settings.offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);

                // Calculate min/max values
                minValue = OctaveValue(minValue, 0, amplitude);
                maxValue = OctaveValue(maxValue, 1, amplitude);

                amplitude *= settings.persistence;
            }

            this.minValue = minValue * settings.multiplier;
            this.maxValue = maxValue * settings.multiplier;

            this.octaveOffsets = octaveOffsets;
        }

        public float Evaluate(Vector2 point)
        {
            float value = 0;
            float amplitude = 1;
            float frequency = 1;

            for (int i = 0; i < settings.octaves; i++)
            {
                float sampleX = (point.x + octaveOffsets[i].x) / settings.scale * frequency;
                float sampleY = (point.y + octaveOffsets[i].y) / settings.scale * frequency;

                float newValue = Mathf.PerlinNoise(sampleX, sampleY);
                value = OctaveValue(value, newValue, amplitude);

                amplitude *= settings.persistence;
                frequency *= settings.lacunarity;
            }

            return value * settings.multiplier;
        }

        private float OctaveValue(float prevValue, float newValue, float amplitude)
        {
            return prevValue + (newValue * amplitude);
        }
    }
}
