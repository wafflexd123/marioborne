using System.Collections;
using UnityEngine;

//someone needs to rewrite this, a hurricane has gone through it multiple times
public class Sword : WeaponBase
{
	[Header("Sword Specific")]
	public Collider[] bladeColliders;
	public Collider[] hiltColliders;
	[Header("Enemy Variables")]
	public float windUpTime;
	public float enemyHitboxTime, jumpWindUpTime, enemyAirTime, cooldownTime;
	public AudioPool.Clip deflectClip;

	Coroutine crtDelay, crtCooldown;
	private Animator animator;
	private float defaultAngularSpeed;
	bool _isFiring;
	ReflectWindow reflectWindow;

	public override bool IsFiring => _isFiring;

	protected override void OnPickup()
	{
		base.OnPickup();

		if (wielder is MeleeAI ai)
		{
			reflectWindow = ai.reflectWindow;
			wielder.model.holdingMelee = true;
			defaultAngularSpeed = ai.agent.angularSpeed;
		}
		else
		{
			reflectWindow = ((Player)wielder).reflectWindowPrefab;
			animator.enabled = true;
		}

		for (int i = 0; i < hiltColliders.Length; i++)
		{
			hiltColliders[i].enabled = false;
		}
		for (int i = 0; i < bladeColliders.Length; i++)
		{
			bladeColliders[i].enabled = false;
			bladeColliders[i].isTrigger = true;
		}
	}

	protected override void OnWielderChange()
	{
		base.OnWielderChange();
		for (int i = 0; i < hiltColliders.Length; i++) hiltColliders[i].enabled = true;
		for (int i = 0; i < bladeColliders.Length; i++)
		{
			bladeColliders[i].enabled = true;
			bladeColliders[i].isTrigger = false;
		}
		if (wielder != null)
		{
			if (wielder is Player)
			{
				animator.Play("unequipped");
				animator.enabled = false;
			}
			else
			{
				wielder.model.holdingMelee = false;
			}
		}
		StopCoroutine(ref crtDelay);//if dropped while attacking
	}

	protected override void Attack()
	{
		if (reflectWindow != null)
		{
			if (wielder is Player) reflectWindow.PlayerReflect(fireDelay);
		}
		if (crtDelay == null)
		{
			if (wielder is Player) animator.Play("swing1");
			fireClips.PlayRandom(audioPool);
			Sound.MakeSound(transform.position, fireClips.clips.Length > 0 ? fireClips.clips[0].maxDistance : 0, wielder);
			crtDelay = StartCoroutine(Delay());
			IEnumerator Delay()
			{
				//THIS NEEDS REWRITING, TOO BUSY RN
				if (wielder is AIController ai)
				{
					if (!ai.enabled) yield break;
					wielder.model.slash = true;
					ai.IsStopped = true;
					ai.RotationSpeed = 0f;
					yield return new WaitForSeconds(windUpTime);
				}
				for (int i = 0; i < bladeColliders.Length; i++) bladeColliders[i].enabled = true;
				_isFiring = true;
				if (wielder is AIController) yield return new WaitForSeconds(enemyHitboxTime);
				else yield return new WaitForSeconds(fireDelay);
				_isFiring = false;
				for (int i = 0; i < bladeColliders.Length; i++) bladeColliders[i].enabled = false;
				if (wielder is AIController ai3)
				{
					if (!ai3.enabled) yield break;
					yield return new WaitUntil(() => AnimStopped());
					ai3.IsStopped = false;
					ai3.RotationSpeed = defaultAngularSpeed;
				}
				crtDelay = null;
			}
		}
	}

	public void Trigger(TriggerCollider triggerCollider)
	{
		if (wielder != null && FindComponent(triggerCollider.other.transform, out Humanoid human)) human.ReceiveAttack(wielder, this, DeathType.Melee, null);
	}

	protected override void Start()
	{
		base.Start();
		animator = GetComponent<Animator>();
		JMEvents.Instance.OnPlayerDeflect += PlayerDeflect;
	}

	private void OnDisable()
	{
		JMEvents.Instance.OnPlayerDeflect -= PlayerDeflect;
	}

	public void JumpAttack()
	{
		if (reflectWindow)
		{
			if (crtDelay == null && crtCooldown == null) crtDelay = StartCoroutine(Delay());
			IEnumerator Delay()
			{
				//THIS NEEDS REWRITING, TOO BUSY RN
				if (wielder is AIController ai)
				{
					reflectWindow.enabled = false;
					wielder.model.jumpAttack = true;
					ai.agent.angularSpeed = 0f; //jumps in straight line, avoiding tracking the player
					yield return new WaitForSeconds(windUpTime);

					for (int i = 0; i < bladeColliders.Length; i++) bladeColliders[i].enabled = true;
					_isFiring = true;
					yield return new WaitForSeconds(enemyAirTime); //roughly the air time
					_isFiring = false;
					for (int i = 0; i < bladeColliders.Length; i++) bladeColliders[i].enabled = false;
					ai.IsStopped = true;
					yield return new WaitUntil(() => AnimStopped()); //time to get free shot in
					ai.IsStopped = false;
					ai.agent.angularSpeed = defaultAngularSpeed;
					reflectWindow.enabled = true;
					if (crtCooldown == null) crtCooldown = StartCoroutine(Cooldown());
					crtDelay = null;
				}
			}

			IEnumerator Cooldown()
			{
				yield return new WaitForSeconds(cooldownTime);
				crtCooldown = null;
			}
		}
	}

	private void PlayerDeflect()
	{
		if (reflectWindow)
		{
			//print("I hear that player deflect is triggered");
			DisableHitbox();
			animator.Play("deflect");
			deflectClip.Play(audioPool);
			// TODO reset cooldown or something
			if (crtDelay != null)
			{
				StopCoroutine(crtDelay);
				crtDelay = null;
			}
		}
	}

	public void EnemyDeflect()
	{
		if (reflectWindow)
		{
			if (wielder is AIController) reflectWindow.EnemyReflect(enemyHitboxTime);
		}

		if (crtDelay == null)
		{
			//deflection sound should go here
			crtDelay = StartCoroutine(Delay());
			IEnumerator Delay()
			{
				if (wielder is AIController ai)
				{
					deflectClip.Play(audioPool);
					Debug.Log("deflecting");
					wielder.model.deflect = true;
					ai.IsStopped = true;
					ai.RotationSpeed = 0f;
					yield return new WaitForSeconds(AnimLength());
					Debug.Log("done deflecting");
					ai.IsStopped = false;
					ai.RotationSpeed = defaultAngularSpeed;
					crtDelay = null;
				}
			}
		}
	}

	public void DisableHitbox()
	{
		foreach (Collider col in bladeColliders)
		{
			col.enabled = false;
		}
	}

	private float AnimLength()
	{
		return wielder.model.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
	}

	private bool AnimStopped()
	{
		return wielder.model.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f;
	}
}
