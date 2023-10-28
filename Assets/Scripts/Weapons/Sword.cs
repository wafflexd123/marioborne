using System.Collections;
using UnityEngine;

public class Sword : WeaponBase
{
	[Header("Sword Specific")]
	public ReflectWindow reflectWindow;
	private bool reflectEnabled;
	public Collider[] bladeColliders, hiltColliders;
	Coroutine crtDelay, crtCooldown, crtRNG;
	private Animator animator;

	[field: Header("Enemy Variables")]
	public float enemyHitboxTime;
	public float enemyAirTime;
	public float recoveryTime;
	public float windUpTime = 0.4f;
	public float cooldownTime = 5f;
	public float rngCooldown = 1f;
	private float defaultAngularSpeed;
	bool _isFiring;

	public override bool IsFiring => _isFiring;

	protected override void OnPickup()
	{
		base.OnPickup();

		if (wielder is AIController ai)
		{
			if (wielder.GetComponent<MeleeAI>().reflectWindow) reflectEnabled = true;
			else reflectEnabled = false;
			wielder.model.holdingMelee = true;
			if (reflectEnabled) reflectWindow = wielder.GetComponent<MeleeAI>().reflectWindow;
			defaultAngularSpeed = ai.agent.angularSpeed;
		}
		if (wielder is Player)
		{
			if (wielder.GetComponent<Player>().reflectWindowPrefab) reflectEnabled = true;
			else reflectEnabled = false;
			if (reflectEnabled) reflectWindow = wielder.GetComponent<Player>().reflectWindowPrefab;
			animator.enabled = true;
		}
		for (int i = 0; i < hiltColliders.Length; i++) hiltColliders[i].enabled = false;
		for (int i = 0; i < bladeColliders.Length; i++)
		{
			bladeColliders[i].enabled = false;
			bladeColliders[i].isTrigger = true;
		}
	}

	protected override void OnDrop()
	{
		for (int i = 0; i < hiltColliders.Length; i++) hiltColliders[i].enabled = true;
		for (int i = 0; i < bladeColliders.Length; i++)
		{
			bladeColliders[i].enabled = true;
			bladeColliders[i].isTrigger = false;
		}

        if (wielder is Player)
        {
            animator.Play("unequipped");
            animator.enabled = false;
        }

    }

	protected override void OnWielderChange()
	{
		base.OnWielderChange();
		if (crtDelay != null)
		{
			StopCoroutine(crtDelay); //if dropped while attacking
			crtDelay = null;
		}
		if (wielder is AIController) wielder.model.holdingMelee = false;
	}

	protected override void Attack()
    {
		if (reflectEnabled)
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
					wielder.model.slash = true;
					ai.IsStopped = true;
					ai.RotationSpeed = 0f;
					yield return new WaitForSeconds(windUpTime);
				}
				for (int i = 0; i < bladeColliders.Length; i++) bladeColliders[i].enabled = true;
				_isFiring = true;
				if(wielder is AIController) yield return new WaitForSeconds(enemyHitboxTime);
				else yield return new WaitForSeconds(fireDelay);
				_isFiring = false;
				for (int i = 0; i < bladeColliders.Length; i++) bladeColliders[i].enabled = false;
				if (wielder is AIController ai3)
				{
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
		if (wielder != null && wielder.gameObject == triggerCollider.gameObject) return;
		if (wielder is AIController)
        {
			if (FindComponent(triggerCollider.other.transform, out Player player))
				player.Kill(DeathType.Melee);
		}
		else if (wielder is Player)
        {
			if (FindComponent(triggerCollider.other.transform, out AIController enemy))
				enemy.Kill(DeathType.Melee);

		}

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
        if (reflectEnabled)
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
        if (reflectEnabled)
        {
			//print("I hear that player deflect is triggered");
			DisableHitbox();
			animator.Play("deflect");
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
		if (reflectEnabled)
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
        foreach (var col in bladeColliders)
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
