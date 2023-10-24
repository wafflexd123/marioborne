using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AIReplacerMB))]
public class AIReplacerEditor : Editor
{
    private AIReplacerMB replacer;

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Replace all AIs in scene"))
        {
            replacer.ReplaceAllInScene();
        }
        if (GUILayout.Button("Replace single"))
        {
            replacer.ReplaceSingle();
        }

        base.OnInspectorGUI();
    }

    private void OnEnable()
    {
        replacer = (AIReplacerMB)target;
    }
}
