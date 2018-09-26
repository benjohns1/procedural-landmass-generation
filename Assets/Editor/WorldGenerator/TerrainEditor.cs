using UnityEngine;
using UnityEditor;

namespace WorldGenerator
{
    [CustomEditor(typeof(TerrainGenerator))]
    public class TerrainEditor : Editor
    {
        TerrainGenerator terrainGenerator;
        Editor biomeEditor;
        Editor heightMapEditor;
        Editor textureEditor;
        Editor meshEditor;

        private void OnEnable()
        {
            terrainGenerator = (TerrainGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (terrainGenerator == null)
            {
                return;
            }
            DrawSettingsEditors();

            if (GUILayout.Button("Generate Terrain"))
            {
                terrainGenerator.GenerateTerrain();
            }
            if (GUILayout.Button("Clear All Terrain Chunks"))
            {
                terrainGenerator.ClearTerrainChunks();
            }
        }
        
        private void DrawSettingsEditors()
        {
            if (terrainGenerator.biomeSettings != null)
            {
                DrawSettingsEditor(terrainGenerator.biomeSettings, ref terrainGenerator.biomeFoldout, ref biomeEditor);

                if (terrainGenerator.biomeSettings.heightSettings != null)
                {
                    DrawSettingsEditor(terrainGenerator.biomeSettings.heightSettings, ref terrainGenerator.heightMapFoldout, ref heightMapEditor);
                }
                if (terrainGenerator.biomeSettings.textureSettings != null)
                {
                    DrawSettingsEditor(terrainGenerator.biomeSettings.textureSettings, ref terrainGenerator.textureFoldout, ref textureEditor);
                }
            }
            if (terrainGenerator.meshSettings != null)
            {
                DrawSettingsEditor(terrainGenerator.meshSettings, ref terrainGenerator.meshFoldout, ref meshEditor);
            }
        }

        void DrawSettingsEditor(Object settings, ref bool foldout, ref Editor editor)
        {
            if (settings == null)
            {
                return;
            }
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (!foldout)
                {
                    return;
                }
                CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();
            }
        }
    }
}