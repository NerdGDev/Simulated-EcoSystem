using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameManager gameMan = (GameManager)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate"))
        {
            gameMan.BuildMapBakers();
        }
    }
}
