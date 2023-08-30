using System;
using UnityEngine;

public enum DeathType { General, Fall, Bullet, Melee }

[SelectionBase]
public abstract class Humanoid : MonoBehaviourPlus, IBulletReceiver
{
    [HideInInspector] public HumanoidAnimatorManager model;
    public UniInput input;

    public abstract Vector3 LookDirection { get; }
    public abstract Vector3 LookingAt { get; }
    public abstract void Kill(DeathType deathType = DeathType.General);
    public abstract bool PickupObject(WeaponBase weapon, out Action onDrop);

    public virtual void OnBulletHit(Collision collision, Bullet bullet)
    {
        if (GetType() != bullet.shooter.GetType())//if shooter and target are not, for example, both an AI (no friendly fire)
        {
            Kill(DeathType.Bullet);
            Destroy(bullet.gameObject);
        }
    }

    protected virtual void Awake()
    {
        input = new UniInput(this);
        Transform t = transform.Find("Body");
        model = t.Find("Model").GetComponent<HumanoidAnimatorManager>();
    }
}