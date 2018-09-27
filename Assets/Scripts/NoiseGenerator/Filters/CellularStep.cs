using UnityEngine;

namespace NoiseGenerator.Filters
{
    public class CellularStep : Filter
    {
        private const int randPrecision = 10000;

        private FilterSettings.CellularStep settings;
        private readonly FastNoise noise;

        public override float GetGlobalMin()
        {
            return 0;
        }

        public override float GetGlobalMax()
        {
            return 1;
        }

        public CellularStep(FilterSettings.CellularStep settings)
        {
            settings.Validate();
            this.settings = settings;
            noise = new FastNoise();
        }

        public override float[,] GenerateRegion(int width, int height, Vector2 startPoint)
        {
            noise.SetSeed(settings.seed);
            noise.SetFrequency(settings.frequency);
            noise.SetCellularDistanceFunction(settings.distanceFunction);
            noise.SetCellularReturnType(settings.returnType);
            if (settings.returnType == FastNoise.CellularReturnType.NoiseLookup)
            {
                FastNoise lookupNoise = new FastNoise(settings.seed);
                lookupNoise.SetNoiseType(FastNoise.NoiseType.Simplex);
                lookupNoise.SetFrequency(settings.frequency);
                noise.SetCellularNoiseLookup(lookupNoise);
            }

            float[,] valueMap = new float[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float val = noise.GetCellular((startPoint.x + x), (startPoint.y - y));
                    float clamped = settings.returnType == FastNoise.CellularReturnType.CellValue ? (val + 1) / 2f : Mathf.Clamp01(val);
                    valueMap[x, y] = settings.cellTypes.Evaluate(clamped).grayscale;
                }
            }

            return valueMap;
        }
    }
}
