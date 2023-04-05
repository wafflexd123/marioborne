using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public abstract class Humanoid : MonoBehaviourPlus
{
	public Transform hand;
	public HumanoidAnimatorManager model;
	public UniInput input;

	public abstract Vector3 LookDirection { get; }
	public abstract Vector3 LookingAt { get; }
	public abstract void Kill();

	protected virtual void Awake()
	{
		input = new UniInput(this);
	}
}