using UnityEngine;
using System.Collections.Generic;

namespace NoiseGenerator
{
    public class NoiseMap
    {
        private NoiseLayer[] layers;
        public float Min { get; private set; }
        public float Max { get; private set; }

        public NoiseMap(NoiseSettings settings)
        {
            float min = 0;
            float max = 0;
            List<NoiseLayer> enabledLayers= new List<NoiseLayer>();
            foreach (NoiseLayer layer in settings.noiseLayers)
            {
                if (!layer.enabled)
                {
                    continue;
                }

                // Instantiate filters
                layer.filter = FilterFactory.CreateFilter(layer.settings);

                // Calculate min/max
                switch (layer.layerOperator)
                {
                    case NoiseLayer.LayerOperator.Set:
                        min = layer.filter.GetMin();
                        max = layer.filter.GetMax();
                        break;
                    case NoiseLayer.LayerOperator.Add:
                        min += layer.filter.GetMin();
                        max += layer.filter.GetMax();
                        break;
                    case NoiseLayer.LayerOperator.Multiply:
                        min *= layer.filter.GetMin();
                        max *= layer.filter.GetMax();
                        break;
                    default:
                        throw new System.Exception("Unknown noise layer operator");
                }

                enabledLayers.Add(layer);
            }

            // Set global min/max values
            this.layers = enabledLayers.ToArray();
            this.Min = min;
            this.Max = max;

            // Call filter setup methods
            foreach (NoiseLayer layer in layers)
            {
                layer.filter.Setup(this.Min, this.Max);
            }
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

            foreach (NoiseLayer layer in layers)
            {
                layer.filter.StartNewRegion(width, height);
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float value = 0;
                    Vector2 point = new Vector2(x + startPoint.x, y - startPoint.y);
                    foreach (NoiseLayer layer in layers)
                    {
                        switch (layer.layerOperator)
                        {
                            case NoiseLayer.LayerOperator.Set:
                                value = layer.filter.Evaluate(point);
                                break;
                            case NoiseLayer.LayerOperator.Add:
                                value += layer.filter.Evaluate(point);
                                break;
                            case NoiseLayer.LayerOperator.Multiply:
                                value *= layer.filter.Evaluate(point);
                                break;
                            default:
                                throw new System.Exception("Unknown noise layer operator");
                        }
                    }
                    noiseMap[x, y] = value;
                }
            }

            return noiseMap;
        }
    }
}
