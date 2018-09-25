using UnityEngine;
using UnityEditor;

namespace WorldGenerator
{
    [CustomEditor(typeof(MapPreview))]
    public class MapPreviewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MapPreview mapPreview = target as MapPreview;
            if (DrawDefaultInspector() && mapPreview.autoUpdate)
            {
                mapPreview.DrawPreviewInEditor();
            }
            if (GUILayout.Button("Generate Preview"))
            {
                mapPreview.DrawPreviewInEditor();
            }
        }
    }
}