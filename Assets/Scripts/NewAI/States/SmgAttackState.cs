using UnityEngine;

public class SmgAttackState : AIState
{
	public float timeBeforePursuingPlayer = 5f, chaseSpeed, maxShootingDistance;

	//Script
	protected Coroutine crtInvestigate;
	float defaultSpeed;
	ExternalControlTransition leaveTransition;

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
	}

	protected override void OnExit()
	{
		StopCoroutine(ref crtInvestigate);
		controller.AgentSpeed = defaultSpeed;
		controller.agent.isStopped = false;
	}

	public override void Tick()
	{
		Debug.Log(Vector3.Distance(controller.player.transform.position, controller.transform.position));
		if(Vector3.Distance(controller.player.transform.position, controller.transform.position) > maxShootingDistance)
        {
			controller.MoveTowards(controller.player.transform.position);
			controller.agent.isStopped = false;
		}
        else
        {
			controller.agent.isStopped = true;
        }

		if (controller.fieldOfView.canSeePlayer)
		{
			controller.LastKnownPlayerPosition = controller.player.transform.position;
			controller.AlertOthers();
			StopCoroutine(ref crtInvestigate);
			controller.RotateTowards(controller.player.transform.position);
			controller.Fire();
		}
		else if (crtInvestigate == null)
		{
			crtInvestigate = StartCoroutine(StandardTimer(timeBeforePursuingPlayer, leaveTransition));
		}
	}
}

