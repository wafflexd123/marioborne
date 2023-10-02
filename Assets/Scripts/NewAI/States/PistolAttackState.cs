using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistolAttackState : AIState
{
	//Inspector
	public float timeBeforeRelocating = 10f;
	public float timeBeforePursuingPlayer = 5f;

	//Script
	Coroutine crtRelocate, crtInvestigate;
	ExternalControlTransition relocate, investigate;

	public override AIState Setup(params Transition[] transitions)
	{
		relocate = (ExternalControlTransition)transitions[1];//currently relocation just swaps the two closest positions. any idea on how we can improve it?
		investigate = (ExternalControlTransition)transitions[0];
		return base.Setup(transitions);
	}

	protected override void OnEntry()
	{
		controller.transform.LookAt(controller.player.transform.position);
	}

	protected override void OnExit()
	{
		StopCoroutine(ref crtInvestigate);
		StopCoroutine(ref crtRelocate);
		controller.coverPointsManager.MarkAsAvailable(controller.currentCoverPoint);
	}

	public override void Tick()
	{
		if (controller.fieldOfView.canSeePlayer)
		{
			controller.transform.LookAt(controller.player.transform.position);
			controller.Fire();
			controller.LastKnownPlayerPosition = controller.player.transform.position;
			controller.AlertOthers();
			if (crtRelocate == null) crtRelocate = StartCoroutine(StandardTimer(timeBeforeRelocating, relocate));
			StopCoroutine(ref crtInvestigate);
		}
		else if (crtInvestigate == null)
		{
			crtInvestigate = StartCoroutine(StandardTimer(timeBeforePursuingPlayer, investigate));
		}
	}
}