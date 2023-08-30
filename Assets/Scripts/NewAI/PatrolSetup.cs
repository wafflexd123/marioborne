using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolSetup : MonoBehaviour
{
    public Transform[] points;

    protected StandardAI standardAI;

    public void SetPatrolRoute()
    {
        if ( standardAI == null )
            standardAI = GetComponent<StandardAI>();
        standardAI.SetPatrolPoints(points);
    }

    private void Awake()
    {
        standardAI = GetComponent<StandardAI>();
    }
}
