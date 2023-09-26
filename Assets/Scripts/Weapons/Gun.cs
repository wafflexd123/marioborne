using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GunAnimator))]
public class Gun : WeaponBase
{
	public float fireDelay, bulletSpeed, reloadDelay;
	public Ammo playerAmmo, aiAmmo;
	public Bullet bulletPrefab;
	public Transform firePosition;
	public AudioClip audioPickup, audioFire;

	protected Ammo ammo;
	new AudioSource audio;
	Coroutine crtDelay, crtReload;
	GameObject ui, qToDrop;
	Image imgReloadPercent;
	TMP_Text txtAmmo;
	GunAnimator animator;

	public override bool IsFiring => false;

	protected override void Start()
	{
		base.Start();
		animator = GetComponent<GunAnimator>();
		ui = transform.Find("UI").gameObject;
		imgReloadPercent = ui.transform.Find("Reload").GetComponent<Image>();
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
		if (wielder is AIController) wielder.model.holdingPistol = true;
		animator.StartAnimations();
		if (wielder is Player)
		{
			ammo = playerAmmo;
			ui.SetActive(true);
			audio.PlayOneShot(audioPickup);
		}
		else
		{
			ammo = aiAmmo;
		}
	}

	protected override void OnWielderChange()
	{
		base.OnWielderChange();
		if (wielder is AIController) wielder.model.holdingPistol = false;
		animator.StopAnimations();
		ui.SetActive(false);
		if (crtDelay != null)
		{
			StopCoroutine(crtDelay); //if dropped while attacking
			crtDelay = null;
		}
	}

	protected override void Attack()
	{
		if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity && ammo.TryFire())//not waiting for fireDelay && wielder is looking at something && can shoot
		{
			if (wielder is Player)
			{
				txtAmmo.text = $"{playerAmmo.amount}";
				if (playerAmmo.amount <= 0) qToDrop.SetActive(true);
				crtDelay = StartCoroutine(DelayWithUI());
				animator.Shoot();
			}
			else
			{
				if (aiAmmo.amount <= 0) crtReload = StartCoroutine(Reload());
				else
				{
					wielder.model.shoot = true;
					crtDelay = StartCoroutine(Delay());
				}
			}
			Sound.MakeSound(transform.position, soundRadius, audioFire, audio, 1, wielder);
			Shoot();
		}
	}

    protected virtual void Shoot()
	{
		Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, DirectionWithSpread(ammo.maxSpread), wielder, ammo.color, false);
	}

	protected Vector3 DirectionWithSpread(float maxSpread)
	{
		if (maxSpread == 0) return (wielder.LookingAt - firePosition.position).normalized;
		else return ((wielder.LookingAt - firePosition.position).normalized + RandomSpread(maxSpread)).normalized;
	}

	protected Vector3 RandomSpread(float maxSpread)
	{
		return maxSpread == 0 ? Vector3.zero : new Vector3(Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread));
	}

	public override void Drop(float dropForce)
	{
		base.Drop(dropForce);
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
		if (wielder is AIController) wielder.model.shoot = false;
		crtDelay = null;
	}

	IEnumerator Delay()
	{
		yield return new WaitForSeconds(fireDelay);
		crtDelay = null;
	}

	IEnumerator Reload()
	{
		yield return new WaitForSeconds(reloadDelay);
		aiAmmo.amount = aiAmmo.startAmount;
		crtReload = null;
	}

	[System.Serializable]
	public class Ammo
	{
		public bool isInfinite;
		public int startAmount;
		public Color color;
		public float maxSpread;
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