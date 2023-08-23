using UnityEngine;

public class Shotgun : Gun
{
	public float shotgunPellets, maxSpread;

	protected override void Shoot()
	{
		for (int i = 1; i < shotgunPellets; i++)
			Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, (wielder.LookingAt - firePosition.position).normalized + new Vector3(Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread)), wielder, ammo.color);
		//also fire a bullet directly ahead
		Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, (wielder.LookingAt - firePosition.position).normalized, wielder, ammo.color);
        MakeSound();
    }
}
