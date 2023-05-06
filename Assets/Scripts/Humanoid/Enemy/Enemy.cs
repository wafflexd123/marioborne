using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Humanoid
{
	public enum EnemyType { Ranged, Melee }

	//Inspector
	public Transform head, points;
	public EnemyType type;
	public float sightRadius, meleeRadius, deathAnimationSpeed, patrolSpeed, meleeSpeed;
	public bool isPatrolling = false, passive = false;

	//Script
	Vector3 lookingAt, velocity = Vector3.zero;
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
		agent.updatePosition = false;
		agent.speed = patrolSpeed;
		agentSpeed = agent.speed;
		fov = GetComponent<FieldOfView>();

        if (hand.childCount > 0)
        {
            if(type == EnemyType.Melee) model.holdingMelee = true;
            if(type == EnemyType.Ranged) model.holdingGun = true;
        }
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
					agent.speed = meleeSpeed;
                    model.holdingMelee = true;
                    agent.SetDestination(Player.singlePlayer.transform.position);
					if (!passive)
					{
						Collider[] meleeRay = Physics.OverlapSphere(transform.position, meleeRadius, 1 << 3);
                        if (meleeRay.Length > 0 && meleeRay[0] != null && FindComponent(meleeRay[0].transform, out Player player))
                        {
                            agent.isStopped = true;
                            if (hand.childCount > 0) input.Press("Mouse", () => -1, () => false);
                        }
                        else agent.isStopped = false;
					}
					break;

				case EnemyType.Ranged:
					agent.isStopped = true;
                    model.holdingGun = true;
                    transform.LookAt(new Vector3(Player.singlePlayer.camera.transform.position.x, transform.position.y, Player.singlePlayer.camera.transform.position.z));
					lookingAt = FirstOrderIntercept(transform.position, Vector3.zero, hand.GetChild(0).GetComponent<Gun>().bulletSpeed, Player.singlePlayer.camera.transform.position, Player.singlePlayer.GetComponent<Rigidbody>().velocity);
					Debug.DrawLine(hand.position, lookingAt);
					if (!passive && hand.childCount > 0)
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
			agent.speed = patrolSpeed;
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

	private void FixedUpdate()
	{
		transform.position = Vector3.SmoothDamp(transform.position, agent.nextPosition, ref velocity, 0.1f);
	}

	void GoToNextPoint()
	{
		isPatrolling = true;
		if (points == null || points.childCount == 0) return;
		agent.destination = points.GetChild(destPoint).position;
		destPoint = (destPoint + 1) % points.childCount;
	}

	public static Vector3 FirstOrderIntercept(Vector3 shooterPos, Vector3 shooterVelocity, float bulletSpeed, Vector3 targetPos, Vector3 targetVelocity)
	{
		Vector3 targetRelativePos = targetPos - shooterPos;
		Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
		float t = FirstOrderInterceptTime(bulletSpeed, targetRelativePos, targetRelativeVelocity);
		return targetPos + t * (targetRelativeVelocity);
	}

	public static float FirstOrderInterceptTime(float bulletSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity)
	{
		float velocitySqr = targetRelativeVelocity.sqrMagnitude;
		if (velocitySqr < 0.001f) return 0f;

		float a = velocitySqr - bulletSpeed * bulletSpeed;

		if (Mathf.Abs(a) < 0.001f)
		{
			float t = -targetRelativePosition.sqrMagnitude / (2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition));
			return Mathf.Max(t, 0f);
		}

		float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
		float c = targetRelativePosition.sqrMagnitude;
		float determinant = b * b - 4f * a * c;

		if (determinant > 0f)
		{
			float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a), t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
			if (t1 > 0f)
			{
				if (t2 > 0f)
				{
					return Mathf.Min(t1, t2);
				}
				else return t1;
			}
			else return Mathf.Max(t2, 0f);
		}
		else if (determinant < 0f)
		{
			return 0f;
		}
		else return Mathf.Max(-b / (2f * a), 0f);
	}

	public override void Kill(DeathType deathType = DeathType.General)
	{
        model.attacking = false;
		model.dying = true;
		if (hand.childCount > 0) input.Press("Drop");//drop weapon if holding one
		model.transform.SetParent(null);
		Destroy(gameObject);//delete everything but the model; saves memory & cpu usage
	}
}
