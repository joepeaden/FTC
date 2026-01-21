using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DeveloperTools))]
public class DeveloperToolsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DeveloperTools developerTools = (DeveloperTools)target;

        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Kill Enemy Pawns"))
        {
            developerTools.KillEnemyPawns();
        }
    }
}
