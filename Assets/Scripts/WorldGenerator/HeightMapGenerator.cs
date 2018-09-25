using NoiseGenerator;
using UnityEngine;

namespace WorldGenerator
{
    public class HeightMapGenerator
    {
        public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 startPoint)
        {
            NoiseMap map = new NoiseMap(settings.noiseSettings);
            float[,] values = map.GenerateRegion(width, height, startPoint);
            return new HeightMap(values, map.Min, map.Max);
        }
    }
}