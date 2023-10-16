using UnityEngine;

public class PatrolState : AIState
{
	public enum StartState { StartAtFirst, StartAtNearest, StartAtRandom }

	//Inspector
	public Transform patrolPoints;
	[Tooltip("Whether the patrol path should be run in reverse when completed, or looping from the 0th patrol point")] public bool pingPong = true;
	public StartState startAt;

	//Script
	[SerializeField, ReadOnly] int patrolIndex;
	int pingpongDirection = 1;

	public override AIState Setup(params Transition[] transitions)
	{
		switch (startAt)
		{
			case StartState.StartAtFirst:
				patrolIndex = 0;
				break;

			case StartState.StartAtNearest:
				float nearest = Mathf.Infinity;
				int nearestTransform = 0;
				for (int i = 0; i < patrolPoints.childCount; i++)
				{
					float temp = Mathf.Abs(Vector3.Distance(transform.position, patrolPoints.GetChild(i).position));
					if (temp < nearest)
					{
						nearest = temp;
						nearestTransform = i;
					}
				}
				patrolIndex = nearestTransform;
				break;

			case StartState.StartAtRandom:
				patrolIndex = Random.Range(0, patrolPoints.childCount);
				break;
		}

		return base.Setup(transitions);
	}

	public override void Tick()
	{
		if (Vector3.Distance(controller.transform.position, patrolPoints.GetChild(patrolIndex).position) < 0.05f)
			IncrementPatrolIndex();
		controller.MoveTowards(patrolPoints.GetChild(patrolIndex).position);
	}

	protected void IncrementPatrolIndex()
	{
		if(patrolPoints.childCount > 1)
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
}