using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Humanoid
{
	public enum EnemyType { Ranged, Melee }
	enum InputAxes { Vertical, Horizontal, Drop, Interact, Mouse }

	//Inspector
	public Transform head;
	public Renderer[] renderers;//temporary, for death animation
	public Transform[] points;
	public EnemyType type;
	public float sightRadius, deathAnimationSpeed;

	//Script
	List<InputAxis> inputAxes = new List<InputAxis>();
	Coroutine crtDeath;
	Vector3 lookingAt;
	private int destPoint = 0;
	private NavMeshAgent agent;

	public override Vector3 LookDirection => head.TransformDirection(Vector3.forward);
	public override Vector3 LookingAt => lookingAt;

	void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		string[] names = Enum.GetNames(typeof(InputAxes));//must be in awake
		for (int i = 0; i < names.Length; i++) inputAxes.Add(new InputAxis(names[i]));//create InputAxis classes from InputAxes enum
	}

	void Update()
	{
		if (crtDeath == null)//if not currently dying
		{
			Collider[] ray = Physics.OverlapSphere(transform.position, sightRadius, 1 << 3);
			if (ray.Length > 0 && ray[0] != null && FindComponent(ray[0].transform, out PlayerMovement player))
			{
				agent.isStopped = true;
				transform.LookAt(player.transform);
				lookingAt = player.transform.position;
				if (hand.childCount > 0) inputAxes[(int)InputAxes.Mouse].Press(-1, this);//if holding something, left click (shoot)
			}
			else
			{
				lookingAt = Vector3.negativeInfinity;
				agent.isStopped = false;
			}
			if (!agent.pathPending && agent.remainingDistance < 0.5f)
			{
				GoToNextPoint();
			}
		}
	}

	void GoToNextPoint()
	{
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
		if (crtDeath == null) crtDeath = StartCoroutine(Animation());
		IEnumerator Animation()
		{
			if (hand.childCount > 0) inputAxes[(int)InputAxes.Drop].Press(1, this);//drop weapon if holding one
			while (true)
			{
				for (int i = 0; i < renderers.Length; i++)
				{
					renderers[i].material.color += new Color(0, 0, 0, -deathAnimationSpeed * Time.deltaTime);
				}
				if (renderers[0].material.color.a <= 0) break;
				yield return null;
			}
			Destroy(gameObject);
		}
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
