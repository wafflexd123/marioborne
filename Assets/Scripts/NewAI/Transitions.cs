using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Transition
{
	public readonly AIState targetState;
	public virtual bool RequirementsMet() { return false; }
	public Transition(AIState targetState) { this.targetState = targetState; }
}

public class ExternalControlTransition : Transition
{
	public bool trigger = false;

	public ExternalControlTransition(AIState targetState) : base(targetState) { }

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

	public StartShootingTransition(AIState targetState) : base(targetState) { }

	public override bool RequirementsMet()
	{
		return Vector3.Distance(destination, position) < 0.05f;
	}
}

public class ReachedLastKnownPlayerPosTransition : Transition
{
	public AIController controller;

	public ReachedLastKnownPlayerPosTransition(AIState targetState, AIController controller) : base(targetState)
	{
		this.controller = controller;
	}

	public override bool RequirementsMet() { return Vector3.Distance(controller.LastKnownPlayerPosition, controller.transform.position) < 1f; }
}

public class FoundPlayerTransition : Transition
{
	public AIController controller;

	public FoundPlayerTransition(AIState targetState, AIController controller) : base(targetState)
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

	public CanSeePlayerTransition(AIState targetState, AIController controller) : base(targetState)
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

	public CanHearPlayerTransition(AIState targetState, AIController controller) : base(targetState)
	{
		this.controller = controller;
	}

	public override bool RequirementsMet()
	{
		return controller.SoundLocation != null;
	}
}

public class InvestigateTransition : Transition
{
	public bool trigger = false;

	public InvestigateTransition(AIState targetState) : base(targetState) { }

	public override bool RequirementsMet()
	{
		bool internalTrigger = trigger;
		trigger = false;
		return internalTrigger;
	}
}
