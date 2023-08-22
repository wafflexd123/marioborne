using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : WeaponBase
{
    public override bool IsFiring => false;

    protected override void OnPickup()
    {
        base.OnPickup();
    }

    protected override void OnWielderChange()
    {
        base.OnWielderChange();
    }

    protected override void Attack()
    {
        base.Attack();
    }
}
