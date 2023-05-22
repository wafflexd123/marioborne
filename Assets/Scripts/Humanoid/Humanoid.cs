using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DeathType { General, Fall, Bullet, Melee }

[SelectionBase]
public abstract class Humanoid : MonoBehaviourPlus
{
	[HideInInspector] public Transform hand;
	[HideInInspector] public HumanoidAnimatorManager model;
	public UniInput input;
	//public Coroutine crtDeflectDelay, crtDeflectTime;
	//public GameObject deflectWindow;

	public abstract Vector3 LookDirection { get; }
	public abstract Vector3 LookingAt { get; }
	public abstract void Kill(DeathType deathType = DeathType.General);

	protected virtual void Awake()
	{
		input = new UniInput(this);
		Transform t = transform.Find("Body");
		hand = t.Find("Hand");
		model = t.Find("Model").GetComponent<HumanoidAnimatorManager>();
	}
}