using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Humanoid, ITimeScaleListener
{
	public enum EnemyType { Ranged, Melee }
	public enum EnemyState { Patrol, Engage, Investigate }

	//Inspector
	[SerializeField] Transform points;
	public float sightRadius, meleeRadius, deathAnimationSpeed, patrolSpeed, meleeSpeed, investigateSpeed, investigateSpotTime, maxInvestigateTime;

	//Script
	Vector3 lookingAt, velocity = Vector3.zero;
	int destPoint = 0;
	float _agentSpeed, maxInvestigateTimer = 0, investigateTimer = 0;
	NavMeshAgent agent;
	FieldOfView fov;
	Transform head;
	EnemyType _typeOfWeapon;

	[Header("Debug (Don't change values)")]
	[SerializeField] EnemyState _state;

	//Properties
	public override Vector3 LookDirection => head.TransformDirection(Vector3.forward);
	public override Vector3 LookingAt => lookingAt;
	public float AgentSpeed { get => _agentSpeed; set { _agentSpeed = value; agent.speed = value * Time.timeScale; } }
	public Transform Points { get => points; set { points = value; FindClosestPoint(); } }
	public EnemyState State
	{
		get => _state; set
		{
			_state = value;
			switch (value)
			{
				case EnemyState.Patrol:
					InitPatrol();
					break;
				case EnemyState.Engage:
					InitEngage();
					break;
				case EnemyState.Investigate:
					InitInvestigate();
					break;
			}
		}
	}
	public EnemyType TypeOfWeapon
	{
		get => _typeOfWeapon;
		set
		{
			_typeOfWeapon = value;
			switch (value)
			{
				case EnemyType.Ranged:
					model.holdingGun = true;
					break;
				case EnemyType.Melee:
					model.holdingMelee = true;
					break;
			}
		}
	}


	protected override void Awake()
	{
		base.Awake();
		agent = GetComponent<NavMeshAgent>();
		agent.updatePosition = false;
		agent.speed = patrolSpeed;
		AgentSpeed = agent.speed;
		State = EnemyState.Patrol;
		fov = GetComponent<FieldOfView>();
		head = transform.Find("Body").Find("Head");
		Time.timeScaleListeners.Add(this);

		if (hand.childCount > 0)
		{
			if (hand.GetChild(0).GetComponent<WeaponBase>() is Gun) TypeOfWeapon = EnemyType.Ranged;
			else TypeOfWeapon = EnemyType.Melee;
		}
	}

	void Update()
	{
		model.velocity = agent.velocity;

		if (fov.canSeePlayer) State = EnemyState.Engage; //changes to engage state once player is spotted

		switch (State)//theoretically more efficient than if/else
		{
			case EnemyState.Patrol:
				Patrol();
				break;
			case EnemyState.Engage:
				Engage();
				break;
			case EnemyState.Investigate:
				Investigate();
				break;
		}
	}

	private void FixedUpdate()
	{
		transform.position = Vector3.SmoothDamp(transform.position, agent.nextPosition, ref velocity, 0.1f);
	}

	void InitEngage()
	{
		switch (TypeOfWeapon)
		{
			case EnemyType.Ranged:
				agent.isStopped = true;
				break;
			case EnemyType.Melee:
				AgentSpeed = meleeSpeed;
				break;
		}
	}

	private void Engage()
	{
		if (!fov.canSeePlayer)
		{
			State = EnemyState.Investigate;
			return;
		}
		switch (TypeOfWeapon)
		{
			case EnemyType.Melee:
				agent.SetDestination(Player.singlePlayer.transform.position);
				if (Vector3.Distance(transform.position, Player.singlePlayer.transform.position) <= meleeRadius)
				{
					agent.isStopped = true;
					input.Press("Mouse", () => -1, () => false);//left click (hit)
				}
				else agent.isStopped = false;
				break;

			case EnemyType.Ranged:
				transform.LookAt(new Vector3(Player.singlePlayer.camera.transform.position.x, transform.position.y, Player.singlePlayer.camera.transform.position.z));
				if (!Player.singlePlayer.hasDied) lookingAt = FirstOrderIntercept(transform.position, Vector3.zero, hand.GetChild(0).GetComponent<Gun>().bulletSpeed, Player.singlePlayer.camera.transform.position, Player.singlePlayer.GetComponent<Rigidbody>().velocity);
				hand.GetChild(0).LookAt(lookingAt);
				input.Press("Mouse", () => -1, () => false);//left click (shoot)
				break;
		}
	}

	void InitPatrol()
	{
		lookingAt = Vector3.negativeInfinity;
		AgentSpeed = patrolSpeed;
		agent.isStopped = false;
		FindClosestPoint();
	}

	private void Patrol()
	{
		if (destPoint != -1 && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
		{
			agent.SetDestination(Points.GetChild(destPoint).position);
			destPoint = (destPoint + 1) % Points.childCount;
		}
	}

	void FindClosestPoint()
	{
		if (Points != null && Points.childCount > 0)
		{
			int closestPoint = 0;
			float dist = Vector3.Distance(transform.position, Points.GetChild(0).position);
			for (int i = 0; i < Points.childCount; i++)
			{
				float tempDist = Vector3.Distance(transform.position, Points.GetChild(i).position);
				if (tempDist < dist) closestPoint = i;
			}
			destPoint = closestPoint;
		}
		else destPoint = -1;
	}

	void InitInvestigate()
	{
		lookingAt = Vector3.negativeInfinity;
		AgentSpeed = investigateSpeed;
		agent.isStopped = false;
		agent.SetDestination(fov.playerLocation);
	}

	private void Investigate() //enemy moves to last seen player location, waits for x seconds, then goes back to patrol
	{
		maxInvestigateTimer += Time.deltaTime;
		if (maxInvestigateTimer >= maxInvestigateTime)//enemy will "forget" the player was there after maxInvestigateTime, even if they never reach the spot
		{
			maxInvestigateTimer = 0;
			State = EnemyState.Patrol;
			return;
		}

		if (agent.remainingDistance <= agent.stoppingDistance)
		{
			investigateTimer += Time.deltaTime;
			if (investigateTimer >= investigateSpotTime)//enemy will watch the spot the player was last seen at for investigateSpotTime, once they arrive at the spot
			{
				investigateTimer = 0;
				State = EnemyState.Patrol;
				return;
			}
		}
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
		model.melee = false;
		model.shooting = false;
		model.dying = true;
		if (hand.childCount > 0) input.Press("Drop");//drop weapon if holding one
		model.transform.SetParent(null);
		Destroy(gameObject);//delete everything but the model; saves memory & cpu usage
	}

	public void OnTimeSlow()
	{
		agent.speed = AgentSpeed * Time.timeScale;
	}
}
