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

	private void Start()
	{
		enabled = true;
	}

	public override void Kill(DeathType deathType = DeathType.General)
	{
		
	}

	public override void OnBulletHit(Collision collision, Bullet bullet)
	{
	
	}

	public override bool OnPickupWeapon(WeaponBase weapon, out Action onDrop)
	{
		onDrop = null;
		return false;
	}

	private void OnDestroy()
	{
		onDestroy.Invoke();
	}
}
