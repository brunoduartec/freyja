using Freyja.Environment;
using UnityEditor;
using UnityEngine;

namespace Freyja.Editor
{
    [CustomEditor(typeof(CoastalGrass))]
    public sealed class CoastalGrassEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("Editar Grama")) GrassPlacementEditorWindow.OpenAndActivate();
        }
    }
}
