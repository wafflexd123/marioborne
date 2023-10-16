using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView), typeof(Rigidbody), typeof(NavMeshAgent)), RequireComponent(typeof(RagdollManager), typeof(BasicRewindable))]
public class AIController : Humanoid, ITimeScaleListener, IRewindListener, ITelekinetic
{
	//Inspector
	public float alertRadius;

	//Properties
	[field: SerializeField][field: ReadOnly] public AIState CurrentState { get; protected set; }
	public Vector3 LastKnownPlayerPosition { get; set; }
	public Vector3? SoundLocation { get; set; }
	public FieldOfView fieldOfView { get; protected set; }
	[HideInInspector] public NavMeshAgent agent { get; set; }
	public new Rigidbody rigidbody { get; protected set; }
	public Player player { get; protected set; }
	public float AgentSpeed { get => agentSpeed; set { agentSpeed = value; agent.speed = value * Time.timeScale; } }
	public float RotationSpeed { get => rotationSpeed; set { rotationSpeed = value; agent.angularSpeed = value * Time.timeScale; } }
	public bool IsStopped { get => isStopped; set { isStopped = value; agent.isStopped = value; } }
	public override Vector3 LookDirection => fieldOfView.eyes.forward;
	public override Vector3 LookingAt => lookingAt;

	[SerializeField, Tooltip("Only needs assigning to enemies who use cover. It's now done in the controller script because it needs access in more than one state.")] public CoverPoints coverPointsManager;

	//Script
	protected Vector3 velocity;
	protected RagdollManager ragdoll;
	BasicRewindable rewind;
	float agentSpeed, rotationSpeed, defaultRotSpeed;
	bool isStopped, isDead;
	int layer;
	[HideInInspector] public Vector3 currentCoverPoint;
	[HideInInspector] public Vector3 lookingAt;
	[HideInInspector] public float defaultSpeed;
	[HideInInspector] public WeaponBase heldWeapon;

	protected override void Awake()
	{
		base.Awake();
		fieldOfView = GetComponent<FieldOfView>();
		agent = GetComponent<NavMeshAgent>();
		rigidbody = GetComponent<Rigidbody>();
		rewind = GetComponent<BasicRewindable>();
		player = Player.singlePlayer;
		layer = gameObject.layer;
		agentSpeed = agent.speed;
		defaultSpeed = agentSpeed;
		RotationSpeed = agent.angularSpeed;
		defaultRotSpeed = RotationSpeed;
		ragdoll = GetComponent<RagdollManager>();
		rigidbody.isKinematic = true;
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
		transform.rotation = Quaternion.Lerp(transform.rotation, rot, agent.angularSpeed * Time.deltaTime);
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
		agent.speed = AgentSpeed * Time.timeScale;
	}

	public void Rewind(float seconds)
	{
	}

	public virtual void StartRewind()
	{
		//input.enableInput = false;
		//agent.ResetPath(); //guys how do we track patrol paths
		if (!isDead)
		{
			agent.enabled = false;
			enabled = false;
		}
	}

	public virtual void StopRewind()
	{
		//input.enableInput = true;
		if (!isDead)
		{
			enabled = true;
			agent.enabled = true;
			agent.angularSpeed = defaultRotSpeed;
		}
	}

	protected virtual void OnDestroy()
	{
		Time.timeScaleListeners.Remove(this);
		Time.rewindListeners.Remove(this);
	}

	public override void Kill(DeathType deathType = DeathType.General)
	{
		isDead = true;
		if (weapon)
		{
			heldWeapon = weapon;
			input.Press("Drop");//drop weapon if holding one
		}
		if (DeathParticlesManager.Current != null) DeathParticlesManager.Current.PlayAtLocation(transform.position);
		agent.enabled = false;
		enabled = false;
		model.dying = true;
		if (transform.parent.TryGetComponent(out EnemyManager e)) e.RegisterDeath();
		//rewind.AddFrameAction(() => ResetDeath());
	}

	public void ResetDeath()
	{
		if (transform.parent.TryGetComponent(out EnemyManager e)) e.DeregisterDeath();
		isDead = false;
		enabled = true;
		agent.enabled = true;
		model.dying = false;
		if (heldWeapon)
		{
			heldWeapon.Pickup(this);
		}
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

	public void TelekineticGrab(Telekinesis t)
	{
		ragdoll.ActivateRagdoll();
		ragdoll.SetRagdollGravity(false);
		ragdoll.ToggleHipFollow();
		rigidbody.useGravity = false;
		gameObject.layer = 17;
	}

	public void TelekineticRelease()
	{
		ragdoll.SetRagdollGravity(true);
		ragdoll.ToggleHipFollow();
		rigidbody.useGravity = true;
		gameObject.layer = layer;
	}
}
