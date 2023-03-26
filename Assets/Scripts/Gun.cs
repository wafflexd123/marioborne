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
        if (crtDelay == null)//if not waiting for fireDelay
        {
            Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, (wielder.raycast.point - firePosition.position).normalized);
            crtDelay = StartCoroutine(Delay());
        }

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(fireDelay);
            crtDelay = null;
        }
    }
}
