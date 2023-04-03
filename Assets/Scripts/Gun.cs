using System.Collections;
using UnityEngine;

public class Gun : WeaponBase
{
    public float fireDelay, bulletSpeed;
    public int ammo;
    public Bullet bulletPrefab;
    public Transform firePosition;
    Coroutine crtDelay;

	protected override void OnPickup()
	{
		base.OnPickup();
        wielder.model.holdingWeapon = true;
	}

	protected override void OnDrop()
	{
        wielder.model.holdingWeapon = false;
	}

	protected override void LeftMouse()
    {
        if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
        {
            Debug.Log("pew pew");
            wielder.model.triggerWeapon = true;
            Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, (wielder.LookingAt - firePosition.position).normalized);
            crtDelay = StartCoroutine(Delay());
        }

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(fireDelay);
            crtDelay = null;
        }
    }
}
