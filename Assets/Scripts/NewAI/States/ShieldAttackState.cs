using UnityEngine;

public class ShieldAttackState : AIState
{
	public float timeBeforePursuingPlayer = 5f, approachSpeed, rotationSpeed, viewRadius, viewAngle, meleeDistance;

	//Script
	protected Coroutine crtInvestigate;
	bool facingPlayer;
	float defaultSpeed;
	float defaultRotSpeed;
	LayerMask targetMask, obstacleMask;
	ExternalControlTransition leaveTransition;

	public override AIState Setup(params Transition[] transitions)
	{
		leaveTransition = (ExternalControlTransition)transitions[0];
		return base.Setup(transitions);
	}

	protected override void OnEntry()
	{
		defaultSpeed = controller.AgentSpeed;
		defaultRotSpeed = controller.RotationSpeed;
		targetMask = 1 << 3;
		obstacleMask = 1 << 0 | 1 << 14;
		controller.AgentSpeed = approachSpeed;
		controller.RotationSpeed = rotationSpeed;
		controller.transform.LookAt(controller.player.transform.position);
	}

	protected override void OnExit()
	{
		StopCoroutine(ref crtInvestigate);
		controller.AgentSpeed = defaultSpeed;
		controller.RotationSpeed = defaultRotSpeed;
		//controller.fieldOfView.viewAngle = defaultViewAngle;
	}

	public override void Tick()
	{
		controller.RotationSpeed = rotationSpeed;
		Collider[] rangeChecks = Physics.OverlapSphere(controller.fieldOfView.eyes.position, viewRadius, targetMask, QueryTriggerInteraction.Ignore);
		if (rangeChecks.Length > 0)
		{ //uses fov code to check whether enemy is roughly facing the player
			foreach(Collider rangeCheck in rangeChecks)
            {
				Vector3 dirToTarget = (rangeCheck.transform.position + new Vector3(0, controller.fieldOfView.eyes.position.y) - controller.fieldOfView.eyes.position).normalized;
				if (Vector3.Angle(controller.fieldOfView.eyes.forward, dirToTarget) < viewAngle / 2)
				{
					if (!Physics.Linecast(controller.fieldOfView.eyes.position, controller.player.camera.transform.position, obstacleMask, QueryTriggerInteraction.Ignore))
					{
						facingPlayer = true;
					}
					else facingPlayer = false;
				}
				else facingPlayer = false;
			}
		}
        else
        {
			facingPlayer = false;
		}

		if (facingPlayer) //if facing the player, walk towards them and shoot
        {
			controller.agent.isStopped = false;
			controller.MoveTowards(controller.player.transform.position);
			if (Vector3.Distance(controller.transform.position, controller.player.transform.position) > meleeDistance)
            {
				controller.Fire();
			}
		}
        else //if not facing the player, stop and slowly rotate (gives time for the player to get around to their back)
        {
			controller.agent.isStopped = true;
			controller.agent.ResetPath();
			controller.RotateTowards(controller.player.transform.position);

		}

		if (controller.fieldOfView.canSeePlayer)
		{
			//controller.MoveTowards(controller.player.transform.position);
			controller.LastKnownPlayerPosition = controller.player.transform.position;
			controller.AlertOthers();
			StopCoroutine(ref crtInvestigate);
			if (Vector3.Distance(controller.transform.position, controller.player.transform.position) < meleeDistance)
			{ //if enemy is within melee distance, melee player (okay i need to figure this one out)
				//controller.Fire();
			}
		}
		else if (crtInvestigate == null)
		{
			crtInvestigate = StartCoroutine(StandardTimer(timeBeforePursuingPlayer, leaveTransition));
		}
	}
}

