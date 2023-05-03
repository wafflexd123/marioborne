using System.Collections;
using TMPro;
using UnityEngine;

public class Gun : WeaponBase
{
	public enum GunType { Pistol, Shotgun }
	public float fireDelay, bulletSpeed, maxSpread;
	public Ammo playerAmmo, aiAmmo;
	public int shotgunPellets;
	public GunType type;
	public Bullet bulletPrefab;
	public Transform firePosition;
	Coroutine crtDelay;
	GameObject ui;
	TextMeshProUGUI txtAmmo;
	Ammo ammo;

	protected override void Start()
	{
		base.Start();
		ui = transform.Find("UI").gameObject;
		txtAmmo = ui.transform.Find("Ammo").GetComponent<TextMeshProUGUI>();
		playerAmmo.amount = playerAmmo.startAmount;
		aiAmmo.amount = aiAmmo.startAmount;
		txtAmmo.text = $"{playerAmmo.amount}";
	}

	protected override void OnPickup()
	{
		base.OnPickup();
		wielder.model.holdingWeapon = true;
		if (wielder is Player)
		{
			ammo = playerAmmo;
			ui.SetActive(true);
		}
		else
		{
			ammo = aiAmmo;
		}
	}

	protected override void OnDrop()
	{
		base.OnDrop();
		wielder.model.holdingWeapon = false;
		ui.SetActive(false);
	}

	protected override void LeftMouse()
	{
		if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity && ammo.TryFire())//if not waiting for fireDelay && wielder is looking at something && can shoot
		{
			if (wielder is Player) txtAmmo.text = $"{playerAmmo.amount}";
			wielder.model.attacking = true;
			crtDelay = StartCoroutine(Delay());
			switch (type)
			{
				case GunType.Pistol:
					Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, (wielder.LookingAt - firePosition.position).normalized, wielder);
					break;

				case GunType.Shotgun:
					for (int i = 0; i <= shotgunPellets; i++)
					{
						Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise
							(bulletSpeed, (wielder.LookingAt - firePosition.position).normalized + new Vector3(Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread)), wielder);
					}
					break;
			}
		}

		IEnumerator Delay()
		{
			yield return new WaitForSeconds(fireDelay);
			crtDelay = null;
		}
	}

	[System.Serializable]
	public class Ammo
	{
		public bool isInfinite;
		public int startAmount;
		[HideInInspector] public int amount;

		public bool TryFire()
		{
			if (amount > 0)
			{
				amount--;
				return true;
			}
			else if (isInfinite) return true;
			return false;
		}
	}
}
