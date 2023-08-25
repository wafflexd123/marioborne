using System.Collections;
using UnityEngine;

public class Knife : WeaponBase
{
    public float hitDelay;
    public Collider[] bladeColliders, hiltColliders;
    Coroutine crtDelay;
    bool _isFiring;

    public override bool IsFiring => _isFiring;

    protected override void OnPickup()
    {
        base.OnPickup();
        wielder.model.holdingMelee = true;
        for (int i = 0; i < hiltColliders.Length; i++) hiltColliders[i].enabled = false;
        for (int i = 0; i < bladeColliders.Length; i++)
        {
            bladeColliders[i].enabled = false;
            bladeColliders[i].isTrigger = true;
        }
    }

    protected override void OnDrop()
    {
        for (int i = 0; i < hiltColliders.Length; i++) hiltColliders[i].enabled = true;
        for (int i = 0; i < bladeColliders.Length; i++)
        {
            bladeColliders[i].enabled = true;
            bladeColliders[i].isTrigger = false;
        }
    }

    protected override void OnWielderChange()
    {
        base.OnWielderChange();
        if (crtDelay != null)
        {
            StopCoroutine(crtDelay); //if dropped while attacking
            crtDelay = null;
        }
        wielder.model.holdingMelee = false;
    }

    protected override void Attack()
    {
        if (crtDelay == null)
        {
            crtDelay = StartCoroutine(Delay());
            IEnumerator Delay()
            {
                wielder.model.slash = true;
                for (int i = 0; i < bladeColliders.Length; i++) bladeColliders[i].enabled = true;
                _isFiring = true;
                yield return new WaitUntil(() => !wielder.model.slash);
                _isFiring = false;
                for (int i = 0; i < bladeColliders.Length; i++) bladeColliders[i].enabled = false;
                yield return new WaitForSeconds(hitDelay);
                crtDelay = null;
            }
        }
    }

    public void Trigger(TriggerCollider triggerCollider)
    {
        if (FindComponent(triggerCollider.other.transform, out Humanoid enemy))
        {
            enemy.Kill(DeathType.Melee);
        }
    }
}