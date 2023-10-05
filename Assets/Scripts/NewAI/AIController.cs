using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView), typeof(Rigidbody), typeof(NavMeshAgent))]
public class AIController : Humanoid, ITimeScaleListener, IRewindListener
{
	//Inspector
	public float alertRadius;
	public float defaultSpeed = 1f;
	public float rotationSpeed = 10f;

	//Properties
	[field: SerializeField][field: ReadOnly] public AIState CurrentState { get; protected set; }
	public Vector3 LastKnownPlayerPosition { get; set; }
	public Vector3? SoundLocation { get; set; }
	public FieldOfView fieldOfView { get; protected set; }
	[HideInInspector] public NavMeshAgent agent { get; set; }
	public new Rigidbody rigidbody { get; protected set; }
	public Player player { get; protected set; }
	public float AgentSpeed { get => defaultSpeed; set { defaultSpeed = value; agent.speed = value * Time.timeScale; } }
	public bool IsStopped { get => isStopped; set { isStopped = value; agent.isStopped = value; } }
	public override Vector3 LookDirection => fieldOfView.eyes.forward;
	public override Vector3 LookingAt => lookingAt;

	[SerializeField, Tooltip("Only needs assigning to enemies who use cover. It's now done in the controller script because it needs access in more than one state.")] public CoverPoints coverPointsManager;

	//Script
	protected Vector3 velocity;
	float agentSpeed;
	bool isStopped;
	bool isDead;
	[HideInInspector] public Vector3 currentCoverPoint;
	[HideInInspector] public Vector3 lookingAt;

	protected override void Awake()
	{
		base.Awake();
		fieldOfView = GetComponent<FieldOfView>();
		agent = GetComponent<NavMeshAgent>();
		rigidbody = GetComponent<Rigidbody>();
		player = Player.singlePlayer;
		Time.timeScaleListeners.Add(this);
		Time.rewindListeners.Add(this);
	}

	void FixedUpdate()
	{
		if (CurrentState.TryTransition(out AIState newState))
			CurrentState = newState;
		else
			CurrentState.Tick();

		model.velocity = agent.velocity;

        Debug.Log($"agentSpeed: {agentSpeed}, Time: {Time.timeScale}, agent.speed: {agent.speed}, speed: {AgentSpeed}");
	}

	public void MoveTowards(Vector3 targetPosition)
	{
		velocity = agent.velocity;
		transform.position = Vector3.SmoothDamp(transform.position, agent.nextPosition, ref velocity, 0.1f);
		agent.SetDestination(targetPosition);
	}

	public void RotateTowards(Vector3 lookTarget)
	{
		Vector3 dir = lookTarget - transform.position;
		dir.y = 0;//This allows the object to only rotate on its y axis
		Quaternion rot = Quaternion.LookRotation(dir);
		transform.rotation = Quaternion.Lerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
	}

	public void Fire()
	{
        if (weapon)
        {
			lookingAt = Player.singlePlayer.camera.transform.position;
			weapon.transform.LookAt(lookingAt);
			input.Press("Attack", () => -1, () => false);
		}
	}

	public void AlertOthers()
	{
		Collider[] alertOthers = Physics.OverlapSphere(transform.position, alertRadius, 1 << 11);
		foreach (Collider alerted in alertOthers)
		{ //creates an overlap sphere around enemy, checks if other enemies are in it and prompts them to investigate
			if (Vector3.Distance(alerted.transform.position, transform.position) > 1f)
			{
				alerted.GetComponentInParent<AIController>().SoundLocation = SoundLocation;
			}
		}
	}

	public void OnTimeSlow()
	{
		if(!isDead && agent != null) agent.speed = AgentSpeed * Time.timeScale;
	}

	public void Rewind(float seconds)
	{
	}

	public void StartRewind()
	{
		input.enableInput = false;
	}

	public void StopRewind()
	{
		input.enableInput = true;
	}

	protected virtual void OnDestroy()
	{
		Time.timeScaleListeners.Add(this);
		Time.rewindListeners.Add(this);
	}

	public override void Kill(DeathType deathType = DeathType.General)
	{
		if (weapon) input.Press("Drop");//drop weapon if holding one
		isDead = true;
		if (DeathParticlesManager.Current != null)
            DeathParticlesManager.Current.PlayAtLocation(transform.position);
		Destroy(gameObject);
	}

	public override bool PickupObject(WeaponBase weapon, out Action onDrop)
	{
		if (!this.weapon)
		{
			this.weapon = weapon;
			onDrop = () => this.weapon = null;
			return true;
		}
		onDrop = null;
		return false;
	}

	public override void OnBulletHit(Collision collision, Bullet bullet)
	{
		if (!typeof(AIController).IsAssignableFrom(bullet.shooter.GetType()))//if not shot by an AI (no friendly fire)
		{
			Kill(DeathType.Bullet);
			if (!bullet.penetrates) Destroy(bullet.gameObject);
		}
	}
}
