using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView), typeof(Rigidbody), typeof(NavMeshAgent))]
public class AIController : Humanoid, ITimeScaleListener
{
	//Inspector
	public float alertRadius;
	public float rotationSpeed = 10f;

	//Properties
	public IAIState CurrentState { get; protected set; }
	public Vector3 LastKnownPlayerPosition { get; set; }
	public FieldOfView fieldOfView { get; protected set; }
	protected NavMeshAgent agent { get; set; }
	public new Rigidbody rigidbody { get; protected set; }
	public Player player { get; protected set; }
	public float AgentSpeed { get => agentSpeed; set { agentSpeed = value; agent.speed = value * Time.timeScale; } }
	public override Vector3 LookDirection => fieldOfView.eyes.forward;
	public override Vector3 LookingAt => lookingAt;

	//Script protected
	protected Vector3 soundLocation;
	protected List<IAIState> states = new List<IAIState>();
	protected Vector3 velocity;

	//Script private
	float agentSpeed;
	Vector3 lookingAt;
	//string stateName;
	//float timeEnteredState = 0f;

	protected override void Awake()
	{
		base.Awake();
		fieldOfView = GetComponent<FieldOfView>();
		agent = GetComponent<NavMeshAgent>();
		rigidbody = GetComponent<Rigidbody>();
		player = Player.singlePlayer;
		Time.timeScaleListeners.Add(this);
	}

	void FixedUpdate()
	{
		model.velocity = agent.velocity;
		bool TransitionedThisFrame = false;
		for (int i = 0; i < CurrentState.transitions.Count; i++)
		{
			if (CurrentState.transitions[i].RequirementsMet()) // transition state
			{
				//print($"{name} is transitioning: \t{i}: {CurrentState.GetType().Name} -> {CurrentState.transitions[i].targetState.GetType().Name}, \t time in state: {UnityEngine.Time.time - timeEnteredState}");
				CurrentState.OnExit();
				CurrentState = CurrentState.transitions[i].targetState;
				CurrentState.OnEntry();
				TransitionedThisFrame = true;
				//string stateName = CurrentState.GetType().Name;
				//timeEnteredState = UnityEngine.Time.time;
				break;
			}
		}
		if (!TransitionedThisFrame)
		{
			CurrentState.Tick();
		}
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
		lookingAt = Player.singlePlayer.camera.transform.position;
		if (weapon)
		{
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
				alerted.GetComponentInParent<AIController>().soundLocation = soundLocation;
			}
		}
	}

	public void OnTimeSlow()
	{
		agent.speed = AgentSpeed * Time.timeScale;
	}

	public override void Kill(DeathType deathType = DeathType.General)
	{
		if (weapon) input.Press("Drop");//drop weapon if holding one
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
}
