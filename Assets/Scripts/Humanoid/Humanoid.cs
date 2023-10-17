using System;
using UnityEngine;

public enum DeathType { General, Fall, Bullet, Melee }

[SelectionBase]
public abstract class Humanoid : MonoBehaviourPlus, IBulletReceiver
{
	[HideInInspector] public HumanoidAnimatorManager model;
	[HideInInspector] public WeaponBase weapon;
	public UniInput input;

	public abstract Vector3 LookDirection { get; }
	public abstract Vector3 LookingAt { get; }
	public abstract void Kill(DeathType deathType = DeathType.General);
	public abstract bool OnPickupWeapon(WeaponBase weapon, out Action onDrop);
	public abstract void OnBulletHit(Collision collision, Bullet bullet);

	protected virtual void Awake()
	{
		input = new UniInput(this);
		Transform t = transform.Find("Body");
		if (t != null)
		{
			t = t.Find("Model");
			if (t != null) model = t.GetComponent<HumanoidAnimatorManager>();
		}
	}
}