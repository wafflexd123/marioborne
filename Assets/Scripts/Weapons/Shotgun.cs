using UnityEngine;

public class Shotgun : Gun
{
    [Header("Shotgun Specific")]
    public float shotgunPellets;
    public float maxSpread;

    protected override void Shoot()
    {
        base.Shoot();//fire bullet directly ahead (if ammo has no base spread)
        for (int i = 1; i < shotgunPellets; i++)
            Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(ammo.bulletSpeed, (DirectionWithSpread(ammo.maxSpread) + RandomSpread(maxSpread)).normalized, wielder, ammo.color, false);
    }
}
