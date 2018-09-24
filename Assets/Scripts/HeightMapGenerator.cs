using UnityEngine;
using NoiseGenerator;

public class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter)
    {
        NoiseMap map = new NoiseMap(settings.noiseSettings);
        float[,] values = map.GenerateRegion(width, height, sampleCenter);
        return new HeightMap(values, map.Min, map.Max);
    }
}
