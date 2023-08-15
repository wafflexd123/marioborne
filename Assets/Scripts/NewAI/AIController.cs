using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView), typeof(Rigidbody))]
public class AIController : MonoBehaviour
{
    protected List<IAIState> states = new List<IAIState>();
    public IAIState CurrentState { get; protected set; }
    public Vector3 LastKnownPlayerPosition { get; protected set; }
    public FieldOfView fieldOfView { get; protected set; }
    public NavMeshAgent agent { get; protected set; }
    public Rigidbody rb { get; protected set; }

    protected virtual void Awake()
    {
        fieldOfView = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool TransitionedThisFrame = false;
        for (int i = 0; i < CurrentState.transitions.Count; i++)
        {
            if (CurrentState.transitions[i].RequirementsMet()) // transition state
            {
                CurrentState.OnExit();
                CurrentState = CurrentState.transitions[i].targetState;
                CurrentState.OnEntry();
                TransitionedThisFrame = true;
                break;
            }
        }
        if (!TransitionedThisFrame)
        {
            CurrentState.Tick();
        }
    }
}
