using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh }
    public DrawMode drawMode;

    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshheightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate = true;
    public TerrainType[] regions;

    public Transform displayObject;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, offset);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0, colorIndex = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++, colorIndex++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[colorIndex] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = displayObject.GetComponent<MapDisplay>();
        switch (drawMode)
        {
            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
                break;
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                break;
            case DrawMode.Mesh:
            default:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshheightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
                break;
        }
    }

    private void OnValidate()
    {
        if (noiseScale < Noise.minScale)
        {
            noiseScale = Noise.minScale;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}
