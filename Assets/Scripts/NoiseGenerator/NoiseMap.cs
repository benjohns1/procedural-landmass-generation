using System.Collections.Generic;
using UnityEngine;

namespace NoiseGenerator
{
    public class NoiseMap
    {
        private NoiseLayer[] layers;
        private NoiseSettings settings;
        public float GlobalMin { get; private set; }
        public float GlobalMax { get; private set; }

        public NoiseMap(NoiseSettings settings)
        {
            this.settings = settings;
            Initialize();
        }

        public void Initialize()
        {
            float min = 0;
            float max = 0;
            List<NoiseLayer> enabledLayers = new List<NoiseLayer>();
            foreach (NoiseLayer layer in settings.noiseLayers)
            {
                if (!layer.enabled)
                {
                    continue;
                }

                // Instantiate filters
                layer.filter = FilterFactory.CreateFilter(layer.settings);

                // Calculate min/max
                float layerMin = layer.filter.GetGlobalMin();
                float layerMax = layer.filter.GetGlobalMax();
                switch (layer.layerOperator)
                {
                    case NoiseLayer.LayerOperator.Set:
                        min = layerMin;
                        max = layerMax;
                        break;
                    case NoiseLayer.LayerOperator.Add:
                        min += layerMin;
                        max += layerMax;
                        break;
                    case NoiseLayer.LayerOperator.Multiply:
                        min *= layerMin;
                        max *= layerMax;
                        break;
                    default:
                        throw new System.Exception("Unknown noise layer operator");
                }

                enabledLayers.Add(layer);
            }

            // Set global min/max values
            this.layers = enabledLayers.ToArray();
            this.GlobalMin = min;
            this.GlobalMax = max;

            // Call filter setup methods
            foreach (NoiseLayer layer in layers)
            {
                layer.filter.Setup(this.GlobalMin, this.GlobalMax);
            }
        }

        public Region GenerateRegion(int width, int height, Vector2 startPoint)
        {
            if (width <= 0)
            {
                width = 1;
            }
            if (height <= 0)
            {
                height = 1;
            }

            float[,] region = new float[width, height];

            foreach (NoiseLayer layer in layers)
            {
                ApplyLayer(ref region, layer, width, height, startPoint);
            }

            return new Region(region, GlobalMin, GlobalMax);
        }

        private static void ApplyLayer(ref float[,] noiseMap, NoiseLayer layer, int width, int height, Vector2 startPoint)
        {
            if (layer.filter.generationMode == Filter.GenerationMode.FullRegion)
            {
                float[,] region = layer.filter.GenerateRegion(width, height, startPoint);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        noiseMap[x, y] = CalculateValue(noiseMap[x, y], region[x, y], layer.layerOperator);
                    }
                }
            }

            switch (layer.filter.generationMode)
            {
                case Filter.GenerationMode.EvaluatePoints:
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            Vector2 point = new Vector2(x + startPoint.x, y - startPoint.y);
                            noiseMap[x, y] = CalculateValue(noiseMap[x, y], layer.filter.Evaluate(point), layer.layerOperator);
                        }
                    }
                    break;
                case Filter.GenerationMode.FullRegion:
                    float[,] region = layer.filter.GenerateRegion(width, height, startPoint);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            noiseMap[x, y] = CalculateValue(noiseMap[x, y], region[x, y], layer.layerOperator);
                        }
                    }
                    break;
                default:
                    throw new System.Exception("Unknown filter generation mode");
            }
        }

        private static float CalculateValue(float currentValue, float newValue, NoiseLayer.LayerOperator op)
        {
            switch (op)
            {
                case NoiseLayer.LayerOperator.Set:
                    return newValue;
                case NoiseLayer.LayerOperator.Add:
                    return currentValue + newValue;
                case NoiseLayer.LayerOperator.Multiply:
                    return currentValue * newValue;
                default:
                    throw new System.Exception("Unknown noise layer operator");
            }
        }
    }
}
