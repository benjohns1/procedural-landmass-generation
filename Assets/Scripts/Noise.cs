using UnityEngine;

public static class Noise
{
    public enum NormalizeMode { Local, Global }

    private const float globalNormalizeAdjustment = 1.75f;
    private const int perlinSampleMin = -100000;
    private const int perlinSampleMax = 100000;
    public const float minScale = 0.00000001f;

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCenter)
    {
        if (mapWidth <= 0)
        {
            mapWidth = 1;
        }
        if (mapHeight <= 0)
        {
            mapHeight = 1;
        }
        settings.ValidateValues();

        float[,] noiseMap = new float[mapWidth, mapHeight];

        MinMaxFloat localNoiseMinMax = new MinMaxFloat();
        Vector2 halfMap = new Vector2(mapWidth / 2f, mapHeight / 2f);

        float amplitude = 1;
        float frequency = 1;
        float maxPossibleHeight = 0;
        Vector2[] octaveOffsets = GenerateOffsets(settings, ref maxPossibleHeight, amplitude, frequency, sampleCenter);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (x - halfMap.x + octaveOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfMap.y + octaveOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistence;
                    frequency *= settings.lacunarity;
                }

                localNoiseMinMax.AddValue(noiseHeight);
                noiseMap[x, y] = noiseHeight;

                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / globalNormalizeAdjustment);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }


        if (settings.normalizeMode == NormalizeMode.Local)
        {
            noiseMap = NormalizeLocal(noiseMap, localNoiseMinMax.Min, localNoiseMinMax.Max);
        }

        return noiseMap;
    }

    public static Vector2[] GenerateOffsets(NoiseSettings settings, ref float maxPossibleHeight, float amplitude, float frequency, Vector2 sampleCenter)
    {
        System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(perlinSampleMin, perlinSampleMax) + settings.offset.x + sampleCenter.x;
            float offsetY = prng.Next(perlinSampleMin, perlinSampleMax) - settings.offset.y - sampleCenter.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistence;
        }

        return octaveOffsets;
    }

    private static float[,] NormalizeLocal(float[,] noiseMap, float min, float max)
    {
        int mapWidth = noiseMap.GetLength(0);
        int mapHeight = noiseMap.GetLength(1);
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(min, max, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}
