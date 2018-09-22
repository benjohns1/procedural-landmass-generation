using UnityEngine;

public class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter)
    {
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCenter);
        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        MinMaxFloat minMax = new MinMaxFloat();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * settings.heightMultiplier;
                minMax.AddValue(values[i, j]);
            }
        }

        return new HeightMap(values, minMax.Min, minMax.Max);
    }
}
