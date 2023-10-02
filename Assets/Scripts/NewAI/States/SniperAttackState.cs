using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperAttackState : AIState
{
	//Inspector
	public float timeBeforePatrol = 5f;

	//Script
	Coroutine crtPatrol;
	ExternalControlTransition patrol;

	public override AIState Setup(params Transition[] transitions)
	{
		//relocate = (ExternalControlTransition)transitions[1];//currently relocation just swaps the two closest positions. any idea on how we can improve it?
		patrol = (ExternalControlTransition)transitions[0];
		return base.Setup(transitions);
	}

	protected override void OnEntry()
	{
		controller.transform.LookAt(controller.player.transform.position);
	}

	protected override void OnExit()
	{
		StopCoroutine(ref crtPatrol);
		//StopCoroutine(ref crtRelocate);
	}

	public override void Tick()
	{
		if (controller.fieldOfView.canSeePlayer)
		{
			controller.transform.LookAt(controller.player.transform.position);
			controller.Fire();
			controller.LastKnownPlayerPosition = controller.player.transform.position;
			controller.AlertOthers();
			//if (crtRelocate == null) crtRelocate = StartCoroutine(StandardTimer(timeBeforeRelocating, relocate));
			StopCoroutine(ref crtPatrol);
		}
        else
        {
			controller.RotateTowards(controller.player.transform.position);
			if (crtPatrol == null)
			{
				crtPatrol = StartCoroutine(StandardTimer(timeBeforePatrol, patrol));
			}
		}
	}
}