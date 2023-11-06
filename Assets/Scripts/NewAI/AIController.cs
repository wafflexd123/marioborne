using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView), typeof(Rigidbody), typeof(NavMeshAgent)), RequireComponent(typeof(RagdollManager), typeof(BasicRewindable))]
public class AIController : Humanoid, ITimeScaleListener, IRewindListener, ITelekinetic
{
	//Inspector
	public AudioPool.Clips deathAudio;
	public float alertRadius;
	[Tooltip("Only needs assigning to enemies who use cover. It's now done in the controller script because it needs access in more than one state.")] public CoverPoints coverPointsManager;

	//Properties
	[field: SerializeField][field: ReadOnly] public AIState CurrentState { get; protected set; }
	public Vector3 LastKnownPlayerPosition { get; set; }
	public Vector3? SoundLocation { get; set; }
	public FieldOfView fieldOfView { get; protected set; }
	public NavMeshAgent agent { get; set; }
	public new Rigidbody rigidbody { get; protected set; }
	public Player player { get; protected set; }
	public float AgentSpeed { get => agentSpeed; set { agentSpeed = value; agent.speed = value * Time.timeScale; } }
	public float RotationSpeed { get => rotationSpeed; set { rotationSpeed = value; agent.angularSpeed = value * Time.timeScale; } }
	public bool IsStopped { get => isStopped; set { isStopped = value; agent.isStopped = value; } }
	public override Vector3 LookDirection => fieldOfView.eyes.forward;
	public override Vector3 LookingAt => lookingAt;
	public override Vector3 Velocity => rigidbody.velocity;

	//Script
	protected Vector3 velocity;
	protected RagdollManager ragdoll;
	protected new AudioPool audio;
	float agentSpeed, rotationSpeed, defaultRotSpeed;
	bool isStopped;
	int layer;
	[HideInInspector] public Vector3 currentCoverPoint, lookingAt;
	[HideInInspector] public float defaultSpeed;

	protected override void Awake()
	{
		base.Awake();
		fieldOfView = GetComponent<FieldOfView>();
		agent = GetComponent<NavMeshAgent>();
		rigidbody = GetComponent<Rigidbody>();
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
			if (weapon is not Sword) weapon.transform.LookAt(lookingAt);
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
		//agent.ResetPath(); //guys how do we track patrol paths
		agent.enabled = false;
		enabled = false;
	}

	public virtual void StopRewind()
	{
		enabled = true;
		agent.enabled = true;
		agent.angularSpeed = defaultRotSpeed;
	}

	protected virtual void OnDestroy()
	{
		Time.timeScaleListeners.Remove(this);
		Time.rewindListeners.Remove(this);
	}

	public override void Kill(DeathType deathType = DeathType.General)
	{
		if (weapon) input.Press("Drop");//drop weapon if holding one
		if (DeathParticlesManager.Current != null) DeathParticlesManager.Current.PlayAtLocation(transform.position);
		Vector3 pos = model.transform.position;
		Debug.Log(pos);
		model.transform.SetParent(transform.parent);
		model.transform.position = pos;
		model.dying = true;
		model.RandomiseAnim();
		deathAudio.PlayRandom(model.audioPool);
		if (transform.parent != null && transform.parent.TryGetComponent(out EnemyManager e)) e.RegisterDeath();
		Destroy(gameObject);
	}

	public override bool OnPickupWeapon(WeaponBase weapon, out Action onDrop)
	{
		onDrop = null;
		return !this.weapon;
	}

	public override void ReceiveAttack(MonoBehaviour attacker, MonoBehaviour weapon, DeathType deathType, Collision collision)
	{
		if (!typeof(AIController).IsAssignableFrom(attacker.GetType()))//if not shot by an AI (no friendly fire)
		{
			Kill(deathType);
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
