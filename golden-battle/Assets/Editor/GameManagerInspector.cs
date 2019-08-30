using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerInspector : Editor{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Regenerate")) {
            GameManager gm = ((GameManager)target);
            gm.CreateMap();
            //StartCoroutine(gm.CreateUnits());
        }
    }
}
