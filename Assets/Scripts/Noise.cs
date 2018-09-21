using UnityEngine;

public static class Noise
{
    private const int perlinSampleMin = -100000;
    private const int perlinSampleMax = 100000;
    public const float minScale = 0.00000001f;

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
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

        Vector2[] octaveOffsets = GenerateOffsets(seed, octaves, offset);
        MinMaxFloat minMaxNoise = new MinMaxFloat();
        Vector2 halfMap = new Vector2(mapWidth / 2f, mapHeight / 2f);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfMap.x) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfMap.y) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                minMaxNoise.AddValue(noiseHeight);
                noiseMap[x, y] = noiseHeight;
            }
        }

        return Normalize(noiseMap, minMaxNoise.Min, minMaxNoise.Max);
    }

    public static Vector2[] GenerateOffsets(int seed, int octaves, Vector2 offset)
    {
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(perlinSampleMin, perlinSampleMax) + offset.x;
            float offsetY = prng.Next(perlinSampleMin, perlinSampleMax) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        return octaveOffsets;
    }

    public static float[,] Normalize(float[,] noiseMap, float min, float max)
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
