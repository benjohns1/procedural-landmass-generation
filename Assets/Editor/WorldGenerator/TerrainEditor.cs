using UnityEngine;
using UnityEditor;

namespace WorldGenerator
{
    [CustomEditor(typeof(TerrainGenerator))]
    public class TerrainEditor : Editor
    {
        TerrainGenerator terrainGenerator;
        Editor heightMapEditor;
        Editor noiseMapEditor;

        private void OnEnable()
        {
            terrainGenerator = (TerrainGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawSettingsEditor(terrainGenerator.heightMapSettings, ref terrainGenerator.heightMapFoldout, ref heightMapEditor);
            if (terrainGenerator.heightMapSettings != null)
            {
                DrawSettingsEditor(terrainGenerator.heightMapSettings.noiseSettings, ref terrainGenerator.noiseMapFoldout, ref noiseMapEditor);
            }

            if (GUILayout.Button("Generate Terrain"))
            {
                terrainGenerator.GenerateTerrainEditorPreview();
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