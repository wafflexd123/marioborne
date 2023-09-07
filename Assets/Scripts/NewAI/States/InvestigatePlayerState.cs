using System.Collections;
using UnityEngine;

public class InvestigatePlayerState : AIState
{
	public float minInvestigateTime = 3f, maxInvestigateTime = 7f;
	Coroutine crtInvestigateTimeout;
	ExternalControlTransition timeoutTransition;

	public override AIState Setup(params Transition[] transitions)
	{
		foreach (Transition transition in transitions)
		{
			if (transition is ExternalControlTransition t)
			{
				timeoutTransition = t;
				break;
			}
		}
		return base.Setup(transitions);
	}

	protected override void OnEntry()
	{
		if (controller.SoundLocation != null) //only follow sound if patrolling or already investigating
		{
			controller.LastKnownPlayerPosition = (Vector3)controller.SoundLocation;
		}
		controller.SoundLocation = null;
		ResetRoutine(StandardTimer(Random.Range(minInvestigateTime, maxInvestigateTime), timeoutTransition), ref crtInvestigateTimeout);
	}

	protected override void OnExit()
	{
		StopCoroutine(ref crtInvestigateTimeout);
	}

	public override void Tick()
	{
		controller.MoveTowards(controller.LastKnownPlayerPosition);
		if (controller.SoundLocation != null) OnEntry(); //while investigating, sounds will give away position
	}
}

