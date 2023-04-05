using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Humanoid
{
	public enum EnemyType { Ranged, Melee }

	//Inspector
	public Transform head;
	public Transform points;
	public EnemyType type;
	public float sightRadius, meleeRadius, deathAnimationSpeed;
	public bool isPatrolling = false;

	//Script
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

		if (hand.childCount > 0) model.holdingWeapon = true;
	}

	void Update()
	{
		agent.speed = agentSpeed * Time.timeScale;
		model.velocity = agent.velocity;
		if (fov.canSeePlayer)
		{
			isPatrolling = false;
			switch (type)
			{
				case EnemyType.Melee:
					agent.SetDestination(Player.singlePlayer.transform.position);
					Collider[] meleeRay = Physics.OverlapSphere(transform.position, meleeRadius, 1 << 3);
					if (meleeRay.Length > 0 && meleeRay[0] != null && FindComponent(meleeRay[0].transform, out Player player))
					{
						if (hand.childCount > 0) input.Press("Mouse", () => -1, () => false);
					}
					break;

				case EnemyType.Ranged:
					agent.isStopped = true;
					transform.LookAt(Player.singlePlayer.transform);
					lookingAt = Player.singlePlayer.camera.transform.position;
					if (hand.childCount > 0)
					{
						hand.GetChild(0).LookAt(lookingAt);
						input.Press("Mouse", () => -1, () => false);//if holding something, left click (shoot)
					}
					break;
			}
		}
		else
		{
			lookingAt = Vector3.negativeInfinity;
			agent.isStopped = false;

			if (!isPatrolling && points != null && points.childCount > 0)
			{
				int closestPoint = 0;
				float dist = Vector3.Distance(transform.position, points.GetChild(0).position);
				for (int i = 0; i < points.childCount; i++)
				{
					float tempDist = Vector3.Distance(transform.position, points.GetChild(i).position);
					if (tempDist < dist) closestPoint = i;
				}
				agent.destination = points.GetChild(closestPoint).position;
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
		if (points == null || points.childCount == 0) return;
		agent.destination = points.GetChild(destPoint).position;
		destPoint = (destPoint + 1) % points.childCount;
	}

	public override void Kill()
	{
		model.dying = true;
		if (hand.childCount > 0) input.Press("Drop");//drop weapon if holding one
		model.transform.SetParent(null);
		Destroy(gameObject);//delete everything but the model; saves memory & cpu usage
	}
}
