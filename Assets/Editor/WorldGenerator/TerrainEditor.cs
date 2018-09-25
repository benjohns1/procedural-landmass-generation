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
            DrawSettingsEditor(terrainGenerator.biomeSettings, ref terrainGenerator.biomeFoldout, ref biomeEditor);
            DrawSettingsEditor(terrainGenerator.biomeSettings.heightSettings, ref terrainGenerator.heightMapFoldout, ref heightMapEditor);
            DrawSettingsEditor(terrainGenerator.biomeSettings.textureSettings, ref terrainGenerator.textureFoldout, ref textureEditor);
            DrawSettingsEditor(terrainGenerator.meshSettings, ref terrainGenerator.meshFoldout, ref meshEditor);

            if (GUILayout.Button("Generate Terrain"))
            {
                terrainGenerator.GenerateTerrain();
            }
            if (GUILayout.Button("Clear All Terrain Chunks"))
            {
                terrainGenerator.ClearTerrainChunks();
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