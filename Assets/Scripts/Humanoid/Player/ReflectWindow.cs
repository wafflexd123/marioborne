using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectWindow : MonoBehaviourPlus, IBulletReceiver
{
	public new Collider collider;
	public Color bulletColor;

	Humanoid player;
	readonly List<Bullet> bullets = new List<Bullet>();

	private void Awake()
	{
		FindComponent(transform, out player);
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
