using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CoverPriority { IgnoreCover, IfNear, RequireCover }

public class NavigateFiringPosState : AIState
{
	//Inspector
	public Transform coverPointsTransform;
	public CoverPriority coverPriority = CoverPriority.IfNear;
	public float runToCoverSpeed, nearCoverDistance = 5f, relocateDistance = 0.5f, timeBeforeReturningToPatrol = 10f;
	[Tooltip("Will start to rotate to look at last known player position when this close to firing position")] public float lookingDistance = 3f;

	//Script
	private int coverIndex, sightCoverIndex;
	protected Vector3 targetLocation;
	protected List<Vector3> coverPoints;
	protected ExternalControlTransition returnToPatrolTransition;
	protected StartShootingTransition startShootingTransition;
	Coroutine crtReturnToPatrolTimer;
	float defaultSpeed;

	public override AIState Setup(params Transition[] transitions)
	{
		if (transitions[0] is ExternalControlTransition)//probably should change how this works
		{
			returnToPatrolTransition = (ExternalControlTransition)transitions[0];
			startShootingTransition = (StartShootingTransition)transitions[1];
		}
		else
		{
			returnToPatrolTransition = (ExternalControlTransition)transitions[1];
			startShootingTransition = (StartShootingTransition)transitions[0];
		}
		return base.Setup(transitions);
	}

	protected override void OnEntry()
	{
		defaultSpeed = controller.AgentSpeed;
		if (coverPriority == CoverPriority.IgnoreCover)
		{
			targetLocation = controller.transform.position;
			return;
		}

		controller.AgentSpeed = runToCoverSpeed;
		coverPoints = new List<Vector3>();
		for (int i = 0; i < coverPointsTransform.childCount - 1; i++)
		{
			if (Vector3.Distance(coverPointsTransform.GetChild(i).position, controller.transform.position) > relocateDistance)
				coverPoints.Add(coverPointsTransform.GetChild(i).position);
		}
		// find closest cover point to go to
		float closestDistance = float.MaxValue;
		float closestSightDistance = float.MaxValue;
		for (int i = 0; i < coverPoints.Count; i++)
		{
			if (Vector3.Distance(controller.transform.position, coverPoints[i]) < closestSightDistance)
			{
				Physics.Raycast(new Vector3(coverPoints[i].x, coverPoints[i].y + 1.6f, coverPoints[i].z), controller.player.transform.position - coverPoints[i], out RaycastHit player, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
				if (player.collider != null && player.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
				{ //tries to find cover where the enemy can also see the player (enemy height taken into account)
					Physics.Raycast(coverPoints[i], controller.player.camera.transform.position - coverPoints[i], out RaycastHit cover, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
					if (cover.collider != null && !cover.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
					{
						sightCoverIndex = i;
						closestSightDistance = Vector3.Distance(controller.transform.position, coverPoints[i]);
					}
				}
			}
			if (Vector3.Distance(controller.transform.position, coverPoints[i]) < closestDistance)
			{
				Physics.Raycast(coverPoints[i], controller.player.camera.transform.position - coverPoints[i], out RaycastHit cover, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
				if (cover.collider != null && !cover.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
				{ //raycast to find out whether cover is valid (cover cannot see player)

					coverIndex = i;
					closestDistance = Vector3.Distance(controller.transform.position, coverPoints[i]);
				}
			}

		}

		if (coverPriority == CoverPriority.RequireCover)
		{
			if (closestSightDistance < float.MaxValue) targetLocation = coverPoints[sightCoverIndex];
			else if (closestDistance < float.MaxValue) targetLocation = coverPoints[coverIndex];
			else targetLocation = controller.transform.position; //if there is no cover, should they just stand in place?
		}
		else // CoverPrioirty.IfNear
		{
			if (closestSightDistance <= nearCoverDistance) targetLocation = coverPoints[sightCoverIndex];
			else if (closestDistance <= nearCoverDistance) targetLocation = coverPoints[coverIndex];
			else targetLocation = controller.transform.position; //if there is no cover, should they just stand in place?
		}
	}

	protected override void OnExit()
	{
		controller.AgentSpeed = defaultSpeed;
		startShootingTransition.destination = Vector3.zero;
		startShootingTransition.position = Vector3.one; //resets requirements, otherwise the guy goes a bit wonky
		if (crtReturnToPatrolTimer != null) StopCoroutine(crtReturnToPatrolTimer);
	}

	public override void Tick()
	{
		controller.MoveTowards(targetLocation);
		startShootingTransition.destination = targetLocation;
		startShootingTransition.position = controller.transform.position;

		if (Vector3.Distance(controller.transform.position, targetLocation) < lookingDistance)
		{
			controller.RotateTowards(controller.LastKnownPlayerPosition);
		}

		//needs work i think
		if (!controller.fieldOfView.canSeePlayer)
			ResetRoutine(StandardTimer(timeBeforeReturningToPatrol, returnToPatrolTransition), ref crtReturnToPatrolTimer);
		else
			StopCoroutine(ref crtReturnToPatrolTimer);
	}
}
