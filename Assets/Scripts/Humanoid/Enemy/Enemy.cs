using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Humanoid, ITimeScaleListener
{
	public enum EnemyType { Ranged, Melee }
	public enum EnemyState { Patrol, Engage, Investigate, Idle }

	//Inspector
	[SerializeField] Transform points;
    [SerializeField] Transform coverPoints;
	public float sightRadius, meleeRadius, alertRadius, deathAnimationSpeed, patrolSpeed, rangedSpeed, meleeSpeed, investigateSpeed, investigateSpotTime, maxInvestigateTime, rangedCloseDistanceMin, rangedCloseDistanceMax, aimAdjustVelocityMagnitude;
    public Transform soundLocation;

	//Script
	public WeaponBase weapon;
	Vector3 lookingAt, velocity = Vector3.zero;
	int destPoint = 0;
    int destCoverPoint = 0;
	float _agentSpeed, maxInvestigateTimer = 0, investigateTimer = 0, rangedCloseDistance;
	NavMeshAgent agent;
	FieldOfView fov;
	Transform head;
	EnemyType typeOfWeapon;
	Player player;
	Coroutine crtRotate;

	[Header("Debug (Don't change values)")]
	[SerializeField] EnemyState _state;

	//Properties
	public override Vector3 LookDirection => head.TransformDirection(Vector3.forward);
	public override Vector3 LookingAt => lookingAt;
	public float AgentSpeed { get => _agentSpeed; set { _agentSpeed = value; agent.speed = value * Time.timeScale; } }
	public Transform Points { get => points; set { points = value; FindClosestPoint(); } }
    public Transform CoverPoints { get => coverPoints; set { coverPoints = value; FindClosestPoint(); } }
    public bool IsStopped { get => agent.isStopped; set { if (agent.isOnNavMesh) agent.isStopped = value; } }
	public EnemyState State
	{
		get => _state;
		set
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
	}

	void Start()
	{
		player = Player.singlePlayer;
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

        //for debugging
        for(int i=0; i<CoverPoints.childCount; i++)
        {
            Physics.Raycast(CoverPoints.GetChild(i).position, player.camera.transform.position - CoverPoints.GetChild(i).position, out RaycastHit hit, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
            if (hit.collider != null && !hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
            {
                Debug.DrawRay(CoverPoints.GetChild(i).position, player.model.transform.position - CoverPoints.GetChild(i).position, Color.green);
            }
            else
            {
                Debug.DrawRay(CoverPoints.GetChild(i).position, player.model.transform.position - CoverPoints.GetChild(i).position, Color.red);
            }
        }
	}

	private void FixedUpdate()
	{
		transform.position = Vector3.SmoothDamp(transform.position, agent.nextPosition, ref velocity, 0.1f);
	}

	void InitEngage()
	{
		switch (typeOfWeapon)
		{
			case EnemyType.Ranged:
                IsStopped = true;
                AgentSpeed = rangedSpeed;
                FindClosestCover();
                rangedCloseDistance = UnityEngine.Random.Range(rangedCloseDistanceMin, rangedCloseDistanceMax);
				break;
			case EnemyType.Melee:
				AgentSpeed = meleeSpeed;
				break;
		}
	}

	private void Engage()
	{
        
		if (!fov.canSeePlayer && Vector3.Distance(transform.position, player.transform.position) > rangedCloseDistanceMin)
		{
			State = EnemyState.Investigate;
			return;
		}
        
		if (!player.hasDied)
		{
			switch (typeOfWeapon)
			{
				case EnemyType.Melee:
					agent.SetDestination(player.transform.position);
					if (Vector3.Distance(transform.position, player.transform.position) <= meleeRadius)
					{
						IsStopped = true;
						input.Press("Mouse", () => -1, () => false);//left click (hit)
					}
					else IsStopped = false;
					break;

				case EnemyType.Ranged:

                    //if(destCoverPoint >= 0)
                    //{
                    //    agent.SetDestination(CoverPoints.GetChild(destCoverPoint).position);
                    //}
                    
					if (player.movement.rigidbody.velocity.magnitude > player.movement.walkForce.velocityAtMaxForce * aimAdjustVelocityMagnitude)
					{
						lookingAt = FirstOrderIntercept(transform.position, Vector3.zero, ((Gun)weapon).bulletSpeed, player.camera.transform.position, player.movement.rigidbody.velocity);
					}
					else
					{
						lookingAt = player.camera.transform.position;
					}

					weapon.transform.LookAt(lookingAt);
					transform.LookAt(player.transform);
                    
					if (Vector3.Distance(transform.position, player.transform.position) <= rangedCloseDistance)
					{
						IsStopped = true;
					}
					else
					{
						agent.SetDestination(player.transform.position);
						IsStopped = false;
					}
                    
					input.Press("Mouse", () => -1, () => false);//left click (shoot)
                    
					break;
			}

			AlertOthers();
		}
	}

	void InitPatrol()
	{
		lookingAt = Vector3.negativeInfinity;
		AgentSpeed = patrolSpeed;
		IsStopped = false;
		FindClosestPoint();
	}

	private void Patrol()
	{
        if (soundLocation != null) State = EnemyState.Investigate; //if a sound is heard, investigate
		if (destPoint == -1) State = EnemyState.Idle;//if there are no points to patrol
		else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
		{
			agent.SetDestination(Points.GetChild(destPoint).position);
			destPoint = (destPoint + 1) % Points.childCount;
		}
	}

	void FindClosestPoint()
	{
		if (Points != null)
		{
			if (points.childCount > 0)
			{
				int closestPoint = 0;
				float dist = Mathf.Infinity;
				for (int i = 0; i < Points.childCount; i++)
				{
					float tempDist = Vector3.Distance(transform.position, Points.GetChild(i).position);
					if (tempDist < dist) closestPoint = i;
				}
				destPoint = closestPoint;
				return;
			}
			else
			{
				agent.SetDestination(Points.position);//if there is only 1 point, go to it and then idle
				ResetRoutine(RotateTowardsPoint(), ref crtRotate);
			}
		}
		destPoint = -1;
	}

    void FindClosestCover()
    {
        if(CoverPoints != null)
        {
            if (coverPoints.childCount > 0)
            {
                int closestPoint = 0;
                float dist = Mathf.Infinity;
                for (int i = 0; i < CoverPoints.childCount; i++)
                {
                    float tempDist = Vector3.Distance(transform.position, CoverPoints.GetChild(i).position);
                    if (tempDist < dist)
                    {
                        Physics.Raycast(CoverPoints.GetChild(i).position, player.camera.transform.position - CoverPoints.GetChild(i).position, out RaycastHit hit, Mathf.Infinity, LayerMask.NameToLayer("Enemy"));
                        if(hit.collider != null && !hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
                        {
                            closestPoint = i;
                        }
                    }
                }
                destCoverPoint = closestPoint;
                return;
            }
            else
            {
                agent.SetDestination(CoverPoints.position);//if there is only 1 point, go to it and then idle
                ResetRoutine(RotateTowardsPoint(), ref crtRotate);
            }
        }
        destCoverPoint = -1;
    }

	IEnumerator RotateTowardsPoint()//temporary, idk how to get them to rotate on their own, and they rotate back anyway so rip
	{
		yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
		transform.rotation = Points.rotation;
	}

	void InitInvestigate()
	{
		lookingAt = Vector3.negativeInfinity;
		AgentSpeed = investigateSpeed;
		IsStopped = false;
		if (soundLocation != null) agent.SetDestination(soundLocation.position);
		else agent.SetDestination(fov.playerLocation);
		soundLocation = null;
	}

	private void Investigate() //enemy moves to last seen player location, waits for x seconds, then goes back to patrol
	{
        if (soundLocation != null) InitInvestigate();
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

	private void AlertOthers()
    {
		Collider[] alertOthers = Physics.OverlapSphere(transform.position, alertRadius);
		foreach (var alerted in alertOthers)
		{ //creates an overlap sphere around enemy, checks if other enemies are in it and prompts them to investigate
			if (alerted.gameObject.layer.Equals(LayerMask.NameToLayer("Enemy")) && Vector3.Distance(alerted.transform.position, transform.position) > 1f)
			{
				alerted.GetComponentInParent<Enemy>().soundLocation = transform;
			}
		}
	}
	private void OnDrawGizmos() //for debugging
	{
		//Gizmos.DrawSphere(transform.position, alertRadius);
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
		if (enabled)//Don't die if disabled; allows player to teleport here without issues
		{
			model.dying = true;
			if (weapon) input.Press("Drop");//drop weapon if holding one
			if (transform.parent != null && transform.parent.parent != null && transform.parent.parent.parent != null)
			{
				model.transform.SetParent(transform.parent.parent.parent);//if enemy is part of an auto-unloading section of the level
			}
			else
			{
				model.transform.SetParent(null);//if enemy is always enabled
			}
			Destroy(gameObject);//delete everything but the model; saves memory & cpu usage
		}
	}

	public void OnTimeSlow()
	{
		agent.speed = AgentSpeed * Time.timeScale;
	}

	private void OnDestroy()
	{
		Time.timeScaleListeners.Remove(this);
	}

	public override bool PickupObject(WeaponBase weapon, out Action onDrop)
	{
		if (!this.weapon)
		{
			this.weapon = weapon;
			typeOfWeapon = weapon is Gun ? EnemyType.Ranged : EnemyType.Melee;
			onDrop = () => this.weapon = null;
			return true;
		}
		onDrop = null;
		return false;
	}
}
