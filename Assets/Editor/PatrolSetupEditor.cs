using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PatrolSetup))]
public class PatrolSetupEditor : Editor
{
    private PatrolSetup patrolSetup;

    private void OnEnable()
    {
        patrolSetup = (PatrolSetup)target;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Set Patrol Route"))
        {
            patrolSetup.SetPatrolRoute();
        }

        base.OnInspectorGUI();
    }
}
