using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridGenerator))]
public class GridGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridGenerator gridGen = (GridGenerator)target;

        if (GUILayout.Button("Generate"))
        {
            gridGen.GenerateGrid();
        }
    }
}