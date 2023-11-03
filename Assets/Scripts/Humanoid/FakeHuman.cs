using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FakeHuman : Humanoid
{
	public UnityEvent onDestroy;

	public override Vector3 LookDirection => Vector3.zero;

	public override Vector3 LookingAt => Vector3.zero;

	public override Vector3 Velocity => Vector3.zero;

	private void Start()
	{
		enabled = true;
	}

	public override void Kill(DeathType deathType = DeathType.General)
	{
		Destroy(gameObject);
	}

	public override bool OnPickupWeapon(WeaponBase weapon)
	{
		return false;
	}

	private void OnDestroy()
	{
		onDestroy.Invoke();
	}

	public override void ReceiveAttack(MonoBehaviour attacker, MonoBehaviour weapon, DeathType deathType, Collision collision)
	{
	}
}
