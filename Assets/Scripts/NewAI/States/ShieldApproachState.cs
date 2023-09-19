using UnityEngine;

public class ShieldApproachState : AIState
{
	public float timeBeforePursuingPlayer = 5f, approachSpeed, rotationSpeed, meleeDistance;

	//Script
	protected Coroutine crtInvestigate;
	float defaultSpeed;
	float defaultRotSpeed;
	ExternalControlTransition leaveTransition;

	public override AIState Setup(params Transition[] transitions)
	{
		leaveTransition = (ExternalControlTransition)transitions[0];
		return base.Setup(transitions);
	}

	protected override void OnEntry()
	{
		defaultSpeed = controller.AgentSpeed;
		defaultRotSpeed = controller.rotationSpeed;
		controller.AgentSpeed = approachSpeed;
		controller.rotationSpeed = rotationSpeed;
		controller.transform.LookAt(controller.player.transform.position);
	}

	protected override void OnExit()
	{
		StopCoroutine(ref crtInvestigate);
		controller.AgentSpeed = defaultSpeed;
		controller.rotationSpeed = defaultRotSpeed;
	}

	public override void Tick()
	{
		controller.MoveTowards(controller.player.transform.position);
		controller.RotateTowards(controller.player.transform.position);
		if (controller.fieldOfView.canSeePlayer)
		{
			controller.LastKnownPlayerPosition = controller.player.transform.position;
			controller.AlertOthers();
			StopCoroutine(ref crtInvestigate);
			if (Vector3.Distance(controller.transform.position, controller.player.transform.position) < meleeDistance)
			{
				controller.Fire();
			}
		}
		else if (crtInvestigate == null)
		{
			crtInvestigate = StartCoroutine(StandardTimer(timeBeforePursuingPlayer, leaveTransition));
		}
	}
}

