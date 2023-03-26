using System.Collections;
using UnityEngine;

public class Gun : WeaponBase
{
    public float fireDelay, bulletSpeed;
    public int ammo;
    public Bullet bulletPrefab;
    public Transform firePosition;
    Coroutine crtDelay;

    protected override void LeftMouse()
    {
        if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
        {
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
