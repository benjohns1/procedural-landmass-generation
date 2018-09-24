using UnityEngine;

namespace NoiseGenerator
{
    public class NoiseMap
    {
        private IFilter[] filters;
        private NoiseSettings settings;
        public float Min { get; private set; }
        public float Max { get; private set; }

        public NoiseMap(NoiseSettings settings)
        {
            this.settings = settings;

            filters = new IFilter[settings.noiseLayers.Length];
            float min = 0;
            float max = 0;
            for (int i = 0; i < settings.noiseLayers.Length; i++)
            {
                if (!settings.noiseLayers[i].enabled)
                {
                    continue;
                }
                filters[i] = FilterFactory.CreateFilter(settings.noiseLayers[i].settings);

                // Calculate min/max
                switch (settings.noiseLayers[i].layerType)
                {
                    case NoiseLayer.LayerType.Set:
                        min = filters[i].GetMin();
                        max = filters[i].GetMax();
                        break;
                    case NoiseLayer.LayerType.Add:
                        min += filters[i].GetMin();
                        max += filters[i].GetMax();
                        break;
                    case NoiseLayer.LayerType.Multiply:
                        min *= filters[i].GetMin();
                        max *= filters[i].GetMax();
                        break;
                }
            }

            this.Min = min;
            this.Max = max;
        }

        public float[,] GenerateRegion(int width, int height, Vector2 startPoint)
        {
            if (width <= 0)
            {
                width = 1;
            }
            if (height <= 0)
            {
                height = 1;
            }

            float[,] noiseMap = new float[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float value = 0;
                    Vector2 point = new Vector2(x + startPoint.x, y - startPoint.y);
                    for (int i = 0; i < filters.Length; i++)
                    {
                        if (filters[i] == null)
                        {
                            continue;
                        }
                        switch (settings.noiseLayers[i].layerType)
                        {
                            case NoiseLayer.LayerType.Set:
                                value = filters[i].Evaluate(point, value);
                                break;
                            case NoiseLayer.LayerType.Add:
                                value += filters[i].Evaluate(point, value);
                                break;
                            case NoiseLayer.LayerType.Multiply:
                                value *= filters[i].Evaluate(point, value);
                                break;
                        }
                    }
                    noiseMap[x, y] = value;
                }
            }

            return noiseMap;
        }
    }
}
