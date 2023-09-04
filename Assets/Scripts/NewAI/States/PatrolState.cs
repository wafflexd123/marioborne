using UnityEngine;

public class PatrolState : AIState
{
	//Inspector
	public Transform patrolPoints;
	[Tooltip("Whether the patrol path should be run in reverse when completed, or looping from the 0th patrol point")] public bool pingPong = true;

	//Script
	int patrolIndex = 0;
	int pingpongDirection = 1;

	public override void Tick()
	{
		if (Vector3.Distance(controller.transform.position, patrolPoints.GetChild(patrolIndex).position) < 0.05f)
			IncrementPatrolIndex();
		controller.MoveTowards(patrolPoints.GetChild(patrolIndex).position);
	}

	protected void IncrementPatrolIndex()
	{
		if (pingPong)
		{
			if (patrolIndex + pingpongDirection == -1 || patrolIndex + pingpongDirection == patrolPoints.childCount)
				pingpongDirection *= -1;
			patrolIndex += pingpongDirection;
		}
		else patrolIndex = (patrolIndex + 1) % patrolPoints.childCount;
	}
}