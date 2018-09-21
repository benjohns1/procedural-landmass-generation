using UnityEngine;

public static class Noise
{
    public enum NormalizeMode { Local, Global }

    private const float globalNormalizeAdjustment = 1.75f;
    private const int perlinSampleMin = -100000;
    private const int perlinSampleMax = 100000;
    public const float minScale = 0.00000001f;

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        if (mapWidth <= 0)
        {
            mapWidth = 1;
        }
        if (mapHeight <= 0)
        {
            mapHeight = 1;
        }
        if (scale <= 0)
        {
            scale = minScale;
        }

        float[,] noiseMap = new float[mapWidth, mapHeight];

        MinMaxFloat localNoiseMinMax = new MinMaxFloat();
        Vector2 halfMap = new Vector2(mapWidth / 2f, mapHeight / 2f);

        float amplitude = 1;
        float frequency = 1;
        float maxPossibleHeight = 0;
        Vector2[] octaveOffsets = GenerateOffsets(seed, octaves, offset, ref maxPossibleHeight, amplitude, frequency, persistence);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfMap.x + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfMap.y + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                localNoiseMinMax.AddValue(noiseHeight);
                noiseMap[x, y] = noiseHeight;
            }
        }

        switch (normalizeMode)
        {
            case NormalizeMode.Local:
                return NormalizeLocal(noiseMap, localNoiseMinMax.Min, localNoiseMinMax.Max);
            case NormalizeMode.Global:
            default:
                return NormalizeGlobal(noiseMap, maxPossibleHeight);
        }
    }

    public static Vector2[] GenerateOffsets(int seed, int octaves, Vector2 offset, ref float maxPossibleHeight, float amplitude, float frequency, float persistence)
    {
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(perlinSampleMin, perlinSampleMax) + offset.x;
            float offsetY = prng.Next(perlinSampleMin, perlinSampleMax) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        return octaveOffsets;
    }

    private static float[,] NormalizeGlobal(float[,] noiseMap, float maxPossibleHeight)
    {
        int mapWidth = noiseMap.GetLength(0);
        int mapHeight = noiseMap.GetLength(1);
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / globalNormalizeAdjustment);
                noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
            }
        }
        return noiseMap;
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
