using System.Collections;
using UnityEngine;

public class Gun : WeaponBase
{
    public enum GunType { Pistol, Shotgun }
    public float fireDelay, bulletSpeed, maxSpread;
    public int ammo, shotgunPellets;
    public GunType type;
    public Bullet bulletPrefab;
    public Transform firePosition;
    Coroutine crtDelay;
    private Player player;

    protected override void OnPickup()
	{
		base.OnPickup();
        wielder.model.holdingGun = true;
        if(wielder.GetComponent<Player>())
        {
            player = wielder.GetComponent<Player>();
            if (type == GunType.Pistol) ammo = 10;
            if (type == GunType.Shotgun) ammo = 2;
        }
	}

	protected override void OnDrop()
	{
        base.OnDrop();
        wielder.model.holdingGun = false;
	}

	protected override void LeftMouse()
    {
        if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
        {
            if(ammo > 0)
            {
                switch (type)
                {
                    case GunType.Pistol:
                        wielder.model.attacking = true;
                        Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, (wielder.LookingAt - firePosition.position).normalized);
                        ammo -= 1;
                        crtDelay = StartCoroutine(Delay());
                        break;

                    case GunType.Shotgun:
                        wielder.model.attacking = true;
                        for (int i = 0; i <= shotgunPellets; i++)
                        {
                            Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise
                                (bulletSpeed, (wielder.LookingAt - firePosition.position).normalized + new Vector3(Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread)));
                        }
                        ammo -= 1;
                        crtDelay = StartCoroutine(Delay());
                        break;
                }
            }
            
        }

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(fireDelay);
            if (wielder) wielder.model.attacking = false;
            crtDelay = null;
        }
    }

    protected override void RightMouse()
    {
        if (wielder.GetComponent<Player>() && crtDelay == null)
        {
            if (player.crtDeflectDelay == null && wielder.LookingAt != Vector3.negativeInfinity)
            {
                wielder.model.deflect = true;
                player.crtDeflectDelay = StartCoroutine(Delay());
            }

            IEnumerator Delay()
            {
                player.deflectWindow.SetActive(true);
                yield return new WaitForSeconds(player.deflectDelay);
                player.deflectWindow.SetActive(false);
                wielder.model.deflect = false;
                player.crtDeflectDelay = null;
            }
        }
    }
}
