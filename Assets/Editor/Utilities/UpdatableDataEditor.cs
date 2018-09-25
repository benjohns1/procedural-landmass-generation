using UnityEngine;
using UnityEditor;

namespace Utilities
{
    [CustomEditor(typeof(UpdatableSettings), true)]
    public class UpdatableDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UpdatableSettings data = target as UpdatableSettings;

            if (GUILayout.Button("Update"))
            {
                data.NotifyOfUpdatedValues();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
