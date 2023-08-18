using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigatePlayerState : IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }

    public void OnEntry()
    {
        
    }

    public void OnExit()
    {
        
    }

    public void Tick()
    {
        controller.MoveTowards(controller.LastKnownPlayerPosition);
    }
}

public class ReachedLastKnownPlayerPosTransition : Transition
{
    public AIController controller;
    public Transform transform;
    public ReachedLastKnownPlayerPosTransition(IAIState targetState, AIController controller, Transform myTransform) : base(targetState)
    {
        this.controller = controller;
        this.transform = myTransform;
    }

    public override bool RequirementsMet() { return Vector3.Distance(controller.LastKnownPlayerPosition, transform.position) < 1f; }
}
