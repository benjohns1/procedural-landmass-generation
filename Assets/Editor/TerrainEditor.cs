using UnityEngine;
using UnityEditor;

namespace NoiseGenerator
{
    [CustomEditor(typeof(TerrainGenerator))]
    public class TerrainEditor : Editor
    {
        TerrainGenerator terrainGenerator;
        Editor noiseEditor;

        private void OnEnable()
        {
            terrainGenerator = (TerrainGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (terrainGenerator.noiseSettings == null)
            {
                return;
            }

            DrawSettingsEditor(terrainGenerator.noiseSettings, terrainGenerator.OnNoiseSettingsUpdated, ref terrainGenerator.noiseSettingsFoldout, ref noiseEditor);
            if (GUILayout.Button("Generate Terrain"))
            {
                terrainGenerator.GenerateTerrainEditorPreview();
            }
            if (GUILayout.Button("Clear All Terrain Chunks"))
            {
                terrainGenerator.ClearTerrainChunks();
            }
        }

        void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
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

                if (check.changed && onSettingsUpdated != null)
                {
                    onSettingsUpdated();
                }
            }
        }
    }
}