using System;
using UnityEngine;

public enum DeathType { General, Fall, Bullet, Melee }

[SelectionBase]
public abstract class Humanoid : MonoBehaviourPlus, IAttackReceiver
{
	[HideInInspector] public HumanoidAnimatorManager model;
	[HideInInspector] public WeaponBase weapon;
	public UniInput input;

	public abstract Vector3 LookDirection { get; }
	public abstract Vector3 LookingAt { get; }
	public abstract Vector3 Velocity { get; }
	public abstract void Kill(DeathType deathType = DeathType.General);
	public abstract bool OnPickupWeapon(WeaponBase weapon);
	public abstract void ReceiveAttack(MonoBehaviour attacker, MonoBehaviour weapon, DeathType deathType, Collision collision);

	protected virtual void Awake()
	{
		input = new UniInput(this);
		Transform t = transform.Find("Body");
		if (t != null)
		{
			t = t.Find("Model");
			if (t != null)
			{
                model = t.GetComponent<HumanoidAnimatorManager>();
            }
            if (model == null)
                model = t.GetChild(0).GetComponent<HumanoidAnimatorManager>();
            //print($"{name},\tFound body, found model transform: {t.name == "Model"},\tmodel null: {model == null}");
        }
	}
}

public interface IAttackReceiver
{
	public void ReceiveAttack(MonoBehaviour attacker, MonoBehaviour weapon, DeathType deathType, Collision collision);
}