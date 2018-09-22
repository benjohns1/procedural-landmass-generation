using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = target as MapGenerator;
        if (DrawDefaultInspector() && mapGen.autoUpdate)
        {
            mapGen.DrawPreviewInEditor();
        }
        if (GUILayout.Button("Generate Preview"))
        {
            mapGen.DrawPreviewInEditor();
        }
    }
}
