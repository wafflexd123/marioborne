using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigatePlayerState : IAIState
{
    public List<Transition> transitions { get; set; }
    public AIController controller { get; set; }

    public void OnEntry()
    {
        if (controller.soundLocation != null) //only follow sound if patrolling or already investigating
        {
            controller.LastKnownPlayerPosition = controller.soundLocation.position;
        }
        controller.soundLocation = null;
    }

    public void OnExit()
    {

    }

    public void Tick()
    {
        controller.MoveTowards(controller.LastKnownPlayerPosition);
        if (controller.soundLocation != null) OnEntry(); //while investigating, sounds will give away position
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

public class FoundPlayerTransition : Transition
{
    public AIController controller;

    public FoundPlayerTransition(IAIState targetState, AIController controller) : base(targetState)
    {
        this.controller = controller;
    }

    public override bool RequirementsMet()
    {
        return controller.fieldOfView.canSeePlayer;
    }
}