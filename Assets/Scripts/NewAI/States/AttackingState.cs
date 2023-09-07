using UnityEngine;

public class AttackingState : AIState
{
	public enum EnemyType { Standard, Melee, Shield }

	//Inspector
	public EnemyType type;
	public float timeBeforePursuingPlayer = 5f, chaseSpeed = 1.5f, meleeDistance = 1f;

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
	}

	public override void Tick()
	{
		controller.MoveTowards(controller.player.transform.position);
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

