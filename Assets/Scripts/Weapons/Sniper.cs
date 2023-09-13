using UnityEngine;

public class Sniper : Gun
{
    protected override void Shoot()
    {
            Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, DirectionWithSpread(ammo.maxSpread), wielder, ammo.color, true);
    }
}
