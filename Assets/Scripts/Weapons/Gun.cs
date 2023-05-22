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
	Image deflectPercent;
	TMP_Text txtAmmo;
	Ammo ammo;

	protected override void Start()
	{
		base.Start();
		ui = transform.Find("UI").gameObject;
		imgReloadPercent = ui.transform.Find("Reload").GetComponent<Image>();
		deflectPercent = ui.transform.Find("Deflect").GetComponent<Image>();
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
		wielder.model.holdingGun = true;
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
		wielder.model.holdingGun = false;
		ui.SetActive(false);
	}

	protected override void LeftMouse()
	{
		if (wielder.crtDeflectTime == null) Shoot();
	}

	protected override void RightMouse() //handles deflection while holding weapon
	{
		if (wielder is Player)
		{
			if (crtDelay == null)
			{
				if (wielder.crtDeflectDelay == null && wielder.LookingAt != Vector3.negativeInfinity)
				{
					if (wielder.crtDeflectTime == null)
					{
						wielder.model.deflect = true;
						wielder.crtDeflectTime = StartCoroutine(Anim());

						IEnumerator Anim()
						{
							wielder.deflectWindow.SetActive(true);
							yield return new WaitForSeconds(wielder.GetComponent<Player>().deflectTime);
							wielder.deflectWindow.SetActive(false);
							wielder.model.deflect = false;
							wielder.crtDeflectDelay = StartCoroutine(Delay());
							wielder.crtDeflectTime = null;
						}
					}
				}

				IEnumerator Delay()
				{
					//yield return new WaitForSeconds(wielder.deflectDelay);
					float timer = 0;
					deflectPercent.gameObject.SetActive(true);
					while (timer < wielder.GetComponent<Player>().deflectDelay)
					{
						timer += Time.fixedDeltaTime;
						deflectPercent.fillAmount = timer / wielder.GetComponent<Player>().deflectDelay;
						yield return new WaitForFixedUpdate();
					}
					deflectPercent.gameObject.SetActive(false);
					wielder.crtDeflectDelay = null;
				}
			}
		}
	}

	protected void Shoot()
	{
		if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity && ammo.TryFire())//if not waiting for fireDelay && wielder is looking at something && can shoot
		{
			if (wielder is Player)
			{
				txtAmmo.text = $"{playerAmmo.amount}";
				if (playerAmmo.amount <= 0) qToDrop.SetActive(true);
			}
			audio.Play();
			//wielder.model.shooting = true;
			crtDelay = StartCoroutine(Delay());
			switch (type)
			{
				case GunType.Pistol:
					Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, (wielder.LookingAt - firePosition.position).normalized, wielder);
					break;

				case GunType.Shotgun:
					for (int i = 0; i <= shotgunPellets; i++)
						Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise
							(bulletSpeed, (wielder.LookingAt - firePosition.position).normalized + new Vector3(Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread)), wielder);
					break;
			}
		}
	}

	IEnumerator Delay()
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