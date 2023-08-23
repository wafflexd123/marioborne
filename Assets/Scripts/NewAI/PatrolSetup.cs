using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolSetup : MonoBehaviour
{
    public Transform[] points;

    protected StandardAI standardAI;

    public void SetPatrolRoute()
    {

    }

    private void Awake()
    {
        standardAI = GetComponent<StandardAI>();
    }
}
