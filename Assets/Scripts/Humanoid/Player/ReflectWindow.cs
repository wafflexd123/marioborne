using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ReflectWindow : MonoBehaviourPlus, IAttackReceiver
{
	public new Collider collider;
	public Color bulletColor;
	public bool hit;

	Coroutine crtDelay;
	readonly List<Bullet> bullets = new List<Bullet>();
	Humanoid owner;

	public ReflectWindow Initialise(Humanoid player)
	{
		this.owner = player;
		owner = player;
		name = "Reflect Window - Player";
		ConstraintSource constraintSource = new ConstraintSource();//why the heck is there no constructor for this
		constraintSource.sourceTransform = player.transform;
		constraintSource.weight = 1;
		ParentConstraint constraint = GetComponent<ParentConstraint>();
		constraint.SetSource(0, constraintSource);
		constraint.constraintActive = true;
		collider.enabled = false;
		enabled = false;
		return this;
	}

	private void OnEnable()
	{
		collider.enabled = true;
	}

	void OnDisable()
	{
		hit = false;
		/*
        for (int i = 0; i < bullets.Count; i++)
        {
            bullets[i].direction = player.LookDirection;
            bullets[i].enabled = true;
        }
        bullets.Clear();
        */
		collider.enabled = false;
	}

	public void ReceiveAttack(MonoBehaviour attacker, MonoBehaviour weapon, DeathType deathType, Collision collision)
	{
		// This can go two ways. A deflect in the direction of the player's crosshair, or straight back towards the shooter.

		//bullet.direction = player.LookDirection;
		//bullet.shooter = this;
		//bullet.color = bulletColor;

		// OR
		if (weapon is Bullet bullet)
		{
			if (!hit)
			{
				hit = true;
				if (bullet.shooter)//bullet.shooter is always going to be true???????
				{
					bullet.direction = (bullet.shooter.transform.position - transform.position).normalized;
					bullet.shooter = this;
					bullet.color = bulletColor;
				}
				else
				{
					bullet.direction = owner.LookDirection;
					bullet.shooter = this;
					bullet.color = bulletColor;
				}
			}

			//hit = false;

			/*
			bullet.enabled = false;
			bullet.shooter = this;
			bullet.color = bulletColor;
			bullets.Add(bullet);
			*/

			// animation
			if (owner is Player)
			{
				//print("Reflect window tells action to trigger");
				JMEvents.Instance.PlayerDeflect();
			}
		}
	}

	//why are there two almost entirely identical methods here
	public void PlayerReflect(float waitTime)
	{
		enabled = true;
		if (crtDelay == null)
		{
			crtDelay = StartCoroutine(Delay());
			IEnumerator Delay()
			{
				yield return new WaitForSeconds(waitTime);
				hit = false;
				enabled = false;
				crtDelay = null;
			}
		}
	}

	public void EnemyReflect(float waitTime)
	{
		collider.enabled = false;
		if (crtDelay == null)
		{
			crtDelay = StartCoroutine(Delay());
			IEnumerator Delay()
			{
				yield return new WaitForSeconds(waitTime);
				hit = false;
				collider.enabled = true;
				crtDelay = null;
			}
		}
	}
}
