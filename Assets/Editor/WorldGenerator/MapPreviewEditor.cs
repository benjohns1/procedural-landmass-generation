using UnityEngine;
using UnityEditor;

namespace WorldGenerator
{
    [CustomEditor(typeof(MapPreview))]
    public class MapPreviewEditor : Editor
    {
        MapPreview mapPreview;
        Editor heightMapEditor;
        Editor textureEditor;
        Editor meshEditor;

        public override void OnInspectorGUI()
        {
            mapPreview = target as MapPreview;
            DrawDefaultInspector();
            DrawSettingsEditor(mapPreview.heightMapSettings, ref mapPreview.heightMapFoldout, ref heightMapEditor);
            DrawSettingsEditor(mapPreview.textureSettings, ref mapPreview.textureFoldout, ref textureEditor);
            DrawSettingsEditor(mapPreview.meshSettings, ref mapPreview.meshFoldout, ref meshEditor);
            if (mapPreview.autoUpdate)
            {
                mapPreview.DrawPreviewInEditor();
            }
            if (GUILayout.Button("Generate Preview"))
            {
                mapPreview.DrawPreviewInEditor();
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