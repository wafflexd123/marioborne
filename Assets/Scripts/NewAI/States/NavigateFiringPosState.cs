using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CoverPriority { IgnoreCover, IfNear, RequireCover }

public class NavigateFiringPosState : AIState
{
	//Inspector
	public CoverPriority coverPriority = CoverPriority.IfNear;
	public float runToCoverSpeed, nearCoverDistance = 5f, relocateDistance = 0.5f, timeBeforeReturningToPatrol = 10f;
	[Tooltip("Will start to rotate to look at last known player position when this close to firing position")] public float lookingDistance = 3f;

	//Script
	private Vector3 cover, sightCover;
	protected Vector3 targetLocation;
	protected Dictionary<Vector3, bool> coverPoints;
	protected ExternalControlTransition returnToPatrolTransition;
	protected StartShootingTransition startShootingTransition;
	Coroutine crtReturnToPatrolTimer;
	protected CoverPoints coverPointsManager;

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
		if (coverPriority == CoverPriority.IgnoreCover)
		{
			targetLocation = controller.transform.position;
			return;
		}

		coverPointsManager = controller.coverPointsManager;
		coverPoints = coverPointsManager.coverPoints;

		controller.AgentSpeed = runToCoverSpeed;

		// find closest cover point to go to
		float closestDistance = float.MaxValue;
		float closestSightDistance = float.MaxValue;
		foreach (KeyValuePair<Vector3, bool> coverPoint in coverPoints)
		{
			Vector3 coverPointPosition = coverPoint.Key;
			bool isAvailable = coverPoint.Value;

			if (coverPointsManager.isCoverPointAvailable(coverPointPosition) && Vector3.Distance(controller.transform.position, coverPointPosition) > relocateDistance)
			{
				if (Vector3.Distance(controller.transform.position, coverPointPosition) < closestSightDistance)
				{
					Physics.Raycast(new Vector3(coverPointPosition.x, coverPointPosition.y + 1.6f, coverPointPosition.z), controller.player.transform.position - coverPointPosition, out RaycastHit player, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
					if (player.collider != null && player.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
					{ //tries to find cover where the enemy can also see the player (enemy height taken into account)
						Physics.Raycast(coverPointPosition, controller.player.camera.transform.position - coverPointPosition, out RaycastHit coverHit, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
						if (coverHit.collider != null && !coverHit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
						{
							sightCover = coverPointPosition;
							closestSightDistance = Vector3.Distance(controller.transform.position, coverPointPosition);
						}
					}
				}
				if (Vector3.Distance(controller.transform.position, coverPointPosition) < closestDistance)
				{
					Physics.Raycast(coverPointPosition, controller.player.camera.transform.position - coverPointPosition, out RaycastHit coverHit, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
					if (coverHit.collider != null && !coverHit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
					{ //raycast to find out whether cover is valid (cover cannot see player)

						cover = coverPointPosition;
						closestDistance = Vector3.Distance(controller.transform.position, coverPointPosition);
					}
				}
			}
		}


		if (coverPriority == CoverPriority.RequireCover)
		{
			if (closestSightDistance < float.MaxValue)
			{
				targetLocation = sightCover;
				coverPointsManager.MarkAsTaken(sightCover);
				controller.currentCoverPoint = sightCover;
			}
			else if (closestDistance < float.MaxValue)
			{
				targetLocation = cover;
				coverPointsManager.MarkAsTaken(cover);
				controller.currentCoverPoint = cover;
			}
			else targetLocation = controller.transform.position; //if there is no cover, should they just stand in place?
		}
		else // CoverPrioirty.IfNear
		{
			if (closestSightDistance <= nearCoverDistance)
			{
				targetLocation = sightCover;
				coverPointsManager.MarkAsTaken(sightCover);
				controller.currentCoverPoint = sightCover;
			}
			else if (closestDistance <= nearCoverDistance)
			{
				targetLocation = cover;
				coverPointsManager.MarkAsTaken(cover);
				controller.currentCoverPoint = cover;
			}
			else targetLocation = controller.transform.position; //if there is no cover, should they just stand in place?
		}
		
	}

	protected override void OnExit()
	{
		controller.AgentSpeed = controller.defaultSpeed;
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

		if (!controller.fieldOfView.canSeePlayer)
			ResetRoutine(StandardTimer(timeBeforeReturningToPatrol, returnToPatrolTransition), ref crtReturnToPatrolTimer);
		else
			StopCoroutine(ref crtReturnToPatrolTimer);
	}
}
