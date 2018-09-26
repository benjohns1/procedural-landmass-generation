using NoiseGenerator;
using UnityEngine;

namespace WorldGenerator
{
    public class MapPreview : MonoBehaviour
    {
        public Renderer textureRenderer;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public enum DrawMode { Mesh, NoiseMap }
        public DrawMode drawMode;

        public Vector2 offset = Vector2.zero;
        public MeshSettings meshSettings;
        public NoiseSettings heightMapSettings;
        public TextureSettings textureSettings;

        public Material terrainMaterial;

        [Range(0, MeshSettings.numSupportedLODs - 1)]
        public int editorPreviewLOD;
        public bool autoUpdate;

        [HideInInspector]
        public bool heightMapFoldout;
        [HideInInspector]
        public bool textureFoldout;
        [HideInInspector]
        public bool meshFoldout;

        public void DrawMesh(MeshData meshData)
        {
            meshFilter.sharedMesh = meshData.CreateMesh();
            meshFilter.gameObject.SetActive(true);
            textureRenderer.gameObject.SetActive(false);
        }

        public void DrawTexture(Texture2D texture)
        {
            textureRenderer.sharedMaterial.mainTexture = texture;
            textureRenderer.gameObject.SetActive(true);
            meshFilter.gameObject.SetActive(false);
        }

        public void DrawPreviewInEditor()
        {
            Region heightMap = RegionGenerator.GenerateRegion(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, offset, true);
            textureSettings.ApplyToMaterial(terrainMaterial);
            textureSettings.UpdateMeshHeights(terrainMaterial, heightMap.minValue, heightMap.maxValue);

            switch (drawMode)
            {
                case DrawMode.Mesh:
                    DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
                    break;
                case DrawMode.NoiseMap:
                    DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
                    break;
            }
        }

        private void OnValuesUpdated()
        {
            if (!Application.isPlaying)
            {
                DrawPreviewInEditor();
            }
        }

        private void OnTextureValuesUpdated()
        {
            textureSettings.ApplyToMaterial(terrainMaterial);
        }

        private void OnValidate()
        {
            if (meshSettings != null)
            {
                meshSettings.OnValuesUpdated -= OnValuesUpdated;
                meshSettings.OnValuesUpdated += OnValuesUpdated;
            }
            if (heightMapSettings != null)
            {
                heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
                heightMapSettings.OnValuesUpdated += OnValuesUpdated;
            }
            if (textureSettings != null)
            {
                textureSettings.OnValuesUpdated -= OnTextureValuesUpdated;
                textureSettings.OnValuesUpdated += OnTextureValuesUpdated;
            }
        }
    }
}