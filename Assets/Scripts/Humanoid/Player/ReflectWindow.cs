using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ReflectWindow : MonoBehaviourPlus, IBulletReceiver
{
	public new Collider collider;
	public Color bulletColor;

	Humanoid player;
	readonly List<Bullet> bullets = new List<Bullet>();

	public ReflectWindow Initialise(Player player)
	{
		this.player = player;
		name = "Player Reflect Window";
		ConstraintSource constraintSource = new ConstraintSource();//why the heck is there no constructor for this
		constraintSource.sourceTransform = player.transform;
		constraintSource.weight = 1;
		ParentConstraint constraint = GetComponent<ParentConstraint>();
		constraint.SetSource(0, constraintSource);
		constraint.constraintActive = true;
		enabled = false;
		return this;
	}

	private void OnEnable()
	{
		collider.enabled = true;
	}

	void OnDisable()
	{
		for (int i = 0; i < bullets.Count; i++)
		{
			bullets[i].direction = player.LookDirection;
			bullets[i].enabled = true;
		}
		bullets.Clear();
		collider.enabled = false;
	}

	public void OnBulletHit(Collision collision, Bullet bullet)
	{
		bullet.enabled = false;
		bullet.shooterType = GetType();
		bullet.color = bulletColor;
		bullets.Add(bullet);
	}
}
