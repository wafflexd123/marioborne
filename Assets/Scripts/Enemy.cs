using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Humanoid
{
	public enum EnemyType { Ranged, Melee }
	enum InputAxes { Vertical, Horizontal, Drop, Interact, Mouse, Ability }

	//Inspector
	public Transform head;
	public Transform[] points;
	public EnemyType type;
	public float sightRadius, meleeRadius, deathAnimationSpeed;
	public bool isPatrolling = false;

	//Script
	List<InputAxis> inputAxes = new List<InputAxis>();
	Vector3 lookingAt;
	int destPoint = 0;
	float agentSpeed;
	NavMeshAgent agent;
	FieldOfView fov;

	public override Vector3 LookDirection => head.TransformDirection(Vector3.forward);
	public override Vector3 LookingAt => lookingAt;

	protected override void Awake()
	{
		base.Awake();
		agent = GetComponent<NavMeshAgent>();
		agentSpeed = agent.speed;
		fov = GetComponent<FieldOfView>();
		foreach (string name in Enum.GetNames(typeof(InputAxes))) inputAxes.Add(new InputAxis(name));
        if (type == EnemyType.Ranged) animatorManager.holdingPistol = true;
        else animatorManager.holdingKnife = true;
    }

	void Update()
	{
		//agent.speed = agentSpeed * Time.timeScale;
        //animatorManager.velocity = agent.velocity;
        if (fov.canSeePlayer)
		{
			isPatrolling = false;
			switch (type)
			{
				case EnemyType.Melee:
					agent.SetDestination(fov.playerRef.transform.position);
					Collider[] meleeRay = Physics.OverlapSphere(transform.position, meleeRadius, 1 << 3);
					if (meleeRay.Length > 0 && meleeRay[0] != null && FindComponent(meleeRay[0].transform, out Player player))
					{
						if (hand.childCount > 0) inputAxes[(int)InputAxes.Mouse].Press(-1, this);
					}
					break;

				case EnemyType.Ranged:
					agent.isStopped = true;
					transform.LookAt(fov.playerRef.transform);
					lookingAt = fov.playerRef.GetComponent<Player>().camera.transform.position;
					if (hand.childCount > 0) inputAxes[(int)InputAxes.Mouse].Press(-1, this);//if holding something, left click (shoot)
					break;
			}
		}
		else
		{
			lookingAt = Vector3.negativeInfinity;
			agent.isStopped = false;

			if (!isPatrolling && points.Length > 0)
			{
				int closestPoint = 0;
				float dist = Vector3.Distance(transform.position, points[0].transform.position);
				for (int i = 0; i < points.Length; i++)
				{
					float tempDist = Vector3.Distance(transform.position, points[i].transform.position);
					if (tempDist < dist) closestPoint = i;
				}
				agent.destination = points[closestPoint].position;
				destPoint = closestPoint;
				isPatrolling = true;
			}
		}
		if (!agent.pathPending && agent.remainingDistance < 0.5f)
		{
			GoToNextPoint();
		}
	}

	void GoToNextPoint()
	{
		isPatrolling = true;
		if (points.Length == 0) return;
		agent.destination = points[destPoint].position;
		destPoint = (destPoint + 1) % points.Length;
	}

	public override bool GetAxisDown(string axis, out float value)
	{
		InputAxis inputAxis = InputAxis.FindAxis(axis, inputAxes);
		value = inputAxis.value;
		return inputAxis.wasPressedThisFrame;
	}

	public override void Kill()
	{
        agent.enabled = false;
        animatorManager.dying = true;
        if (hand.childCount > 0) inputAxes[(int)InputAxes.Drop].Press(1, this);//drop weapon if holding one
		enabled = false;
	}

	public class InputAxis
	{
		public readonly string axis;
		public float value;
		public bool wasPressedThisFrame;

		public InputAxis(string axis)
		{
			this.axis = axis;
		}

		public static InputAxis FindAxis(string axis, List<InputAxis> list)
		{
			return list.Find(x => x.axis == axis);
		}

		public void Press(float axisValue, MonoBehaviour monoBehaviour)
		{
			value = axisValue;
			wasPressedThisFrame = true;
			monoBehaviour.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForEndOfFrame();
				wasPressedThisFrame = false;
			}
		}
	}
}
