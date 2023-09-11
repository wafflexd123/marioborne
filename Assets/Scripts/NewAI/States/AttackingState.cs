using UnityEngine;

public class AttackingState : AIState
{
	public float timeBeforePursuingPlayer = 5f, chaseSpeed, meleeDistance;

	//Script
	protected Coroutine crtInvestigate;
	float defaultSpeed;
	ExternalControlTransition leaveTransition;
	public ReflectWindow reflectWindow;

	public override AIState Setup(params Transition[] transitions)
	{
		leaveTransition = (ExternalControlTransition)transitions[0];
		return base.Setup(transitions);
	}

	protected override void OnEntry()
	{
		defaultSpeed = controller.AgentSpeed;
		controller.AgentSpeed = chaseSpeed;
		controller.transform.LookAt(controller.player.transform.position);
		reflectWindow.enabled = true;
	}

	protected override void OnExit()
	{
		StopCoroutine(ref crtInvestigate);
		controller.AgentSpeed = defaultSpeed;
		reflectWindow.enabled = false;
	}

	public override void Tick()
	{
		controller.MoveTowards(controller.player.transform.position);
		if (controller.fieldOfView.canSeePlayer)
		{
			controller.LastKnownPlayerPosition = controller.player.transform.position;
			controller.AlertOthers();
			StopCoroutine(ref crtInvestigate);
			if (Vector3.Distance(controller.transform.position, controller.player.transform.position) < meleeDistance || reflectWindow.hit)
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

