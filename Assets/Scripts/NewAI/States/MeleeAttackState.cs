using UnityEngine;

public class MeleeAttackState : AIState
{
	public float timeBeforePursuingPlayer = 5f, chaseSpeed, meleeDistance, minJumpDistance, maxJumpDistance;

	//Script
	protected Coroutine crtInvestigate, crtJumpCooldown;
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
		if (reflectWindow) reflectWindow.enabled = true;
	}

	protected override void OnExit()
	{
		StopCoroutine(ref crtInvestigate);
		controller.AgentSpeed = defaultSpeed;
		if (reflectWindow) reflectWindow.enabled = false;
	}

	public override void Tick()
	{
		float distance = Vector3.Distance(controller.transform.position, controller.player.transform.position);
		if (reflectWindow && distance > minJumpDistance && distance < maxJumpDistance)
		{
			controller.MoveTowards(controller.LastKnownPlayerPosition);
			controller.GetComponentInChildren<Sword>().JumpAttack();
		}
        else
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
				if (reflectWindow)
				{
					if (reflectWindow.hit)
					{
						controller.Fire();
					}
				}
			}
			else if (crtInvestigate == null)
			{
				crtInvestigate = StartCoroutine(StandardTimer(timeBeforePursuingPlayer, leaveTransition));
			}
		}
	}
}

