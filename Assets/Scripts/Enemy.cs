using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Humanoid
{
	public enum EnemyType
	{
		Ranged,
		Melee
	}

	public Transform[] points;
	private int destPoint = 0;
	private NavMeshAgent agent;

	public EnemyType type;
	public float sightRadius;
	enum InputAxes { Vertical, Horizontal, Drop, Interact, Mouse };
	List<InputAxis> inputAxes = new List<InputAxis>();

	void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		string[] names = Enum.GetNames(typeof(InputAxes));//must be in awake
		for (int i = 0; i < names.Length; i++) inputAxes.Add(new InputAxis(names[i]));//create InputAxis classes from InputAxes enum
	}

	void Update()
	{
		Collider[] ray = Physics.OverlapSphere(transform.position, sightRadius, 1 << 3);
		if (ray.Length > 0 && ray[0] != null && FindComponent(ray[0].transform, out PlayerMovement player))
		{
			agent.isStopped = true;
			transform.LookAt(player.transform);

			if (Physics.Raycast(transform.position, ray[0].transform.position - transform.position, out raycast, sightRadius)) //temporary raycast for later functionality. atm it just tells the gun where to shoot. it is also not very reliable atm.
			{
				if (hand.childCount > 0) inputAxes[(int)InputAxes.Mouse].Press(-1, this);//if holding something, left click (shoot)
			}
		}
		else agent.isStopped = false;
		if (!agent.pathPending && agent.remainingDistance < 0.5f)
		{
			GoToNextPoint();
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
				yield return null;
				wasPressedThisFrame = false;
			}
		}
	}
}
