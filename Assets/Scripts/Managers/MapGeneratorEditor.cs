using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Structures"))
        {
            mapGen.BuildStructures();
        }

        if (GUILayout.Button("Clear Structures"))
        {
            mapGen.ClearStructure();
        }

        if (GUILayout.Button("Generate Environment"))
        {
            mapGen.BuildEnvironment();
        }

        if (GUILayout.Button("Generate Octree Bakers"))
        {
            mapGen.BuildMapBakers();
        }

        if (GUILayout.Button("Clear Octree Bakers"))
        {
            mapGen.ClearMapBakers();
        }
    }
}
