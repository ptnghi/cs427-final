using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(TileMap))]
public class TileMapInspector : Editor{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Regenerate")) {
            TileMap tileMap = (TileMap)target;
            tileMap.BuildMesh();
        }
    }
}
