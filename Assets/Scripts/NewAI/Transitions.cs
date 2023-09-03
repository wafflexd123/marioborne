using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Transition
{
    public IAIState targetState { get; private set; }
    public virtual bool RequirementsMet() { return false; }
    public Transition(IAIState targetState) { this.targetState = targetState; }
}

public class ExternalControlTransition : Transition
{
    public bool trigger = false;

    public ExternalControlTransition(IAIState targetState) : base(targetState) { }

    public override bool RequirementsMet()
    {
        bool internalTrigger = trigger;
        trigger = false;
        return internalTrigger;
    }
}

public class StartShootingTransition : Transition
{
    public Vector3 destination = Vector3.zero;
    public Vector3 position = Vector3.one;

    public StartShootingTransition(IAIState targetState) : base(targetState) { }

    public override bool RequirementsMet()
    {
        return Vector3.Distance(destination, position) < 0.05f;
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

public class CanSeePlayerTransition : Transition
{
    public AIController controller;

    public CanSeePlayerTransition(IAIState targetState, AIController controller) : base(targetState)
    {
        this.controller = controller;
    }

    public override bool RequirementsMet()
    {
        return controller.fieldOfView.canSeePlayer;
    }
}

public class CanHearPlayerTransition : Transition
{
    public AIController controller;

    public CanHearPlayerTransition(IAIState targetState, AIController controller) : base(targetState)
    {
        this.controller = controller;
    }

    public override bool RequirementsMet()
    {
        return controller.soundLocation != null;
    }
}

public class InvestigateTransition : Transition
{
    public bool trigger = false;

    public InvestigateTransition(IAIState targetState) : base(targetState) { }

    public override bool RequirementsMet()
    {
        bool internalTrigger = trigger;
        trigger = false;
        return internalTrigger;
    }
}
