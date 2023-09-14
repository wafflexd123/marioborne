using System.Collections;
using UnityEngine;

public class Knife : WeaponBase
{
	public ReflectWindow reflectWindow;
	public float hitDelay;
	public Collider[] bladeColliders, hiltColliders;
	Coroutine crtDelay;
	bool _isFiring;
	private Animator animator;

	public override bool IsFiring => _isFiring;

	protected override void OnPickup()
	{
		base.OnPickup();
		if (wielder is AIController)
		{
			wielder.model.holdingMelee = true;
			reflectWindow = wielder.GetComponent<MeleeAI>().reflectWindow;
		}
		if (wielder is Player)
		{
			reflectWindow = wielder.GetComponent<Player>().reflectWindowPrefab;
			animator.enabled = true;
			animator.Play("idle");
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
		if (wielder is AIController) reflectWindow.EnemyReflect(hitDelay);
		if (wielder is Player) reflectWindow.PlayerReflect(hitDelay);
		if (crtDelay == null)
		{
            if (wielder is Player) 
				animator.Play("swing1");
			crtDelay = StartCoroutine(Delay());
			IEnumerator Delay()
			{
				//THIS NEEDS REWRITING, TOO BUSY RN
				if (wielder is AIController ai)
				{
					wielder.model.slash = true;
					ai.IsStopped = true;
				}
				for (int i = 0; i < bladeColliders.Length; i++) bladeColliders[i].enabled = true;
				_isFiring = true;
				//if (wielder is AIController ai2)
				//{
				//	yield return new WaitUntil(() => !wielder.model.slash);
				//}
				//else yield return new WaitForSeconds(0.5f); //placeholder
				_isFiring = false;
				for (int i = 0; i < bladeColliders.Length; i++) bladeColliders[i].enabled = false;
				yield return new WaitForSeconds(hitDelay);
				if (wielder is AIController ai2) ai2.IsStopped = false;
				crtDelay = null;
			}
		}
		
	}

	public void Trigger(TriggerCollider triggerCollider)
	{
		if (wielder != null)
			print($"Sword on: {wielder.name} is attacking: {triggerCollider.name}");
		if (wielder != null && wielder.gameObject == triggerCollider.gameObject) return;
		if (FindComponent(triggerCollider.other.transform, out Humanoid enemy))
		{
			enemy.Kill(DeathType.Melee);
		}
	}

    protected override void Start() 
	{
		base.Start();
		animator = GetComponent<Animator>();
	}
}
