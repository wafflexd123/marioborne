using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Gun : WeaponBase
{
	public enum GunType { Pistol, Shotgun }
	public float fireDelay, bulletSpeed, maxSpread;
	public Ammo playerAmmo, aiAmmo;
	public int shotgunPellets;
	public GunType type;
	public Bullet bulletPrefab;
	public Transform firePosition;

	new AudioSource audio;
	Coroutine crtDelay;
	GameObject ui, qToDrop;
	Image imgReloadPercent;
	TMP_Text txtAmmo;
	Ammo ammo;

	public override bool IsFiring => false;

	protected override void Start()
	{
		base.Start();
		ui = transform.Find("UI").gameObject;
		imgReloadPercent = ui.transform.Find("Reload").GetComponent<Image>();
		//deflectPercent = ui.transform.Find("Deflect").GetComponent<Image>();
		txtAmmo = ui.transform.Find("Ammo").GetComponent<TMP_Text>();
		qToDrop = ui.transform.Find("Q To Drop").gameObject;
		playerAmmo.amount = playerAmmo.startAmount;
		aiAmmo.amount = aiAmmo.startAmount;
		txtAmmo.text = $"{playerAmmo.amount}";
		audio = GetComponent<AudioSource>();
	}

	protected override void OnPickup()
	{
		base.OnPickup();
		wielder.model.holdingPistol = true;
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
		wielder.model.holdingPistol = false;
		ui.SetActive(false);
		if (crtDelay != null)
		{
			StopCoroutine(crtDelay); //if dropped while attacking
			crtDelay = null;
		}
	}

	protected override void LeftMouse()
	{
		if (wielder is not Player || !((Player)wielder).fists.IsFiring) Shoot();
		//if (wielder.crtDeflectTime == null) Shoot();
	}

	protected void Shoot()
	{
		if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity && ammo.TryFire())//if not waiting for fireDelay && wielder is looking at something && can shoot
		{
			if (wielder is Player)
			{
				txtAmmo.text = $"{playerAmmo.amount}";
				if (playerAmmo.amount <= 0) qToDrop.SetActive(true);
				crtDelay = StartCoroutine(DelayWithUI());
			}
			else
			{
				crtDelay = StartCoroutine(Delay());
			}
			audio.Play();
			wielder.model.shoot = true;
			switch (type)
			{
				case GunType.Shotgun:
					for (int i = 1; i < shotgunPellets; i++)
						Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).
							Initialise(bulletSpeed, (wielder.LookingAt - firePosition.position).normalized + new Vector3(Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread)), wielder);
					goto case GunType.Pistol;//also fire 1 bullet directly ahead
				case GunType.Pistol:
					Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, (wielder.LookingAt - firePosition.position).normalized, wielder);
					break;
			}
		}
	}

	IEnumerator DelayWithUI()
	{
		float timer = 0;
		imgReloadPercent.gameObject.SetActive(true);
		while (timer < fireDelay)
		{
			timer += Time.fixedDeltaTime;
			imgReloadPercent.fillAmount = timer / fireDelay;
			yield return new WaitForFixedUpdate();
		}
		imgReloadPercent.gameObject.SetActive(false);
		//if (wielder) wielder.model.shooting = false;
		crtDelay = null;
	}

	IEnumerator Delay()
	{
		yield return new WaitForSeconds(fireDelay);
		crtDelay = null;
	}

	[System.Serializable]
	public class Ammo
	{
		public bool isInfinite;
		public int startAmount;
		[HideInInspector] public int amount;

		public bool TryFire()
		{
			if (isInfinite) return true;
			else if (amount > 0)
			{
				amount--;
				return true;
			}
			return false;
		}
	}
}