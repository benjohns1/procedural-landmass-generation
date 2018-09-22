using System.Linq;
using UnityEngine;

[CreateAssetMenu()]
public class TextureSettings : UpdatableData
{
    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;

    public Layer[] layers;

    float savedMinHeight;
    float savedMaxHeight;

    public void ApplyToMaterial(Material material)
    {
        material.SetInt("layerCount", layers.Length);
        material.SetColorArray("baseColors", layers.Select(l => l.tint).ToArray());
        material.SetFloatArray("baseStartHeights", layers.Select(l => l.startHeight).ToArray());
        material.SetFloatArray("baseBlends", layers.Select(l => l.blendStrength).ToArray());
        material.SetFloatArray("baseColorStrength", layers.Select(l => l.tintStrength).ToArray());
        material.SetFloatArray("baseTextureScales", layers.Select(l => l.textureScale).ToArray());
        Texture2DArray texturesArray = GenerateTextureArray(layers.Select(l => l.texture).ToArray());
        material.SetTexture("baseTextures", texturesArray);

        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;

        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

    Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, false);
        for (int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }
        textureArray.Apply();
        return textureArray;
    }

    [System.Serializable]
    public class Layer
    {
        public Texture2D texture;
        public Color tint;
        [Range(0, 1)]
        public float tintStrength;
        [Range(0, 1)]
        public float startHeight;
        [Range(0, 1)]
        public float blendStrength;
        public float textureScale = 1;
    }
}
