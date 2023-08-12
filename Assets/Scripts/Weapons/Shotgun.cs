using UnityEngine;

public class Shotgun : Gun
{
	public float shotgunPellets, maxSpread;

	protected override void Shoot()
	{
		base.Shoot();//fire bullet directly ahead (if ammo has no base spread)
		for (int i = 1; i < shotgunPellets; i++)
			Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, (DirectionWithSpread(ammo.maxSpread) + RandomSpread(maxSpread)).normalized, wielder, ammo.color);
	}
}
