using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(GunAnimator))]
public class Gun : WeaponBase
{
	[Header("Gun Specific")]
	public AudioPool.Clips bulletCasingClips;
	[Range(0f, 1f)] public float bulletCasingSoundChance;
	public float bulletCasingSoundDelay = 1f, reloadDelay;
	public bool penetrates;
	public Ammo playerAmmo, aiAmmo;
	public Bullet bulletPrefab;
	public Transform firePosition;
	public UnityEvent OnFireEvent;

	protected Ammo ammo;
	Coroutine crtDelay, crtReload;
	AmmoCounter ammoCounter;
	GunAnimator animator;

	public override bool IsFiring => false;

	protected override void Start()
	{
		base.Start();
		animator = GetComponent<GunAnimator>();
		ammoCounter = transform.Find("Ammo Counter").GetComponent<AmmoCounter>();
		playerAmmo.amount = playerAmmo.startAmount;
		aiAmmo.amount = aiAmmo.startAmount;

		if (!Application.isEditor) playerAmmo.isInfinite = false;
	}

	protected override void OnPickup()
	{
		base.OnPickup();
		if (wielder is AIController)
		{
			ammo = aiAmmo;
			wielder.model.holdingPistol = true;
		}
		else if (wielder is Player)
		{
			animator.StartAnimations();
			ammoCounter.gameObject.SetActive(true);
			ammoCounter.SetAmmo(playerAmmo.amount);
			ammo = playerAmmo;
			animator.StartAnimations();
		}
	}

	protected override void OnWielderChange()
	{
		base.OnWielderChange();
		if (wielder != null)
		{
			if (wielder is AIController) wielder.model.holdingPistol = false;
			else
			{
				animator.StopAnimations();
				ammoCounter.gameObject.SetActive(false);
			}
			StopCoroutine(ref crtDelay);//if dropped while attacking
		}
	}

	protected override void Attack()
	{
		if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity) //not waiting for fireDelay && wielder is looking at something && can shoot
		{
			if (ammo.TryFire())
			{
				if (wielder is Player)
				{
					ammoCounter.SetAmmo(playerAmmo.amount);
					animator.Shoot();
					crtDelay = StartCoroutine(Delay());
				}
				else
				{
					if (aiAmmo.amount <= 0 && crtReload == null) crtReload = StartCoroutine(Reload());
					else crtDelay = StartCoroutine(Delay());
				}
				if (OnFireEvent != null) OnFireEvent.Invoke();
				fireClips.PlayRandom(audioPool);
				BulletCasingSound();
				Sound.MakeSound(transform.position, fireClips.clips.Length > 0 ? fireClips.clips[0].maxDistance : 0, wielder);
				Shoot();
			}
			else Drop();
		}
	}

	protected virtual void Shoot()
	{
		Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(ammo.bulletSpeed, DirectionWithSpread(ammo.maxSpread), wielder, ammo.color, penetrates);
	}

	protected virtual void BulletCasingSound()
	{
		if (Random.Range(0f, 1f) <= bulletCasingSoundChance) StartCoroutine(E());
		IEnumerator E()
		{
			yield return new WaitForSeconds(bulletCasingSoundDelay);
			bulletCasingClips.PlayRandom(audioPool);
		}
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
		public float maxSpread, bulletSpeed;
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