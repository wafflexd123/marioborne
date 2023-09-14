using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySwapLauncher : MonoBehaviourPlus, IPlayerPower
{
	public Bullet projectile;
	public float projectileSpeed, fireDelay;
	public Transform firePosition;
	Player player;
	Coroutine crtDelay;
	List<Bullet> bullets = new List<Bullet>();

	public bool CanDisable => true;

	private void Awake()
	{
		if (!FindComponent(transform, out player))
		{
			Debug.LogError("Bodyswapper could not find player!", this);
		}
	}

	private void Update()
	{
		if (Input.GetButtonDown("Ability")) Shoot();
	}

	public void Shoot()
	{
		if (crtDelay == null) crtDelay = StartCoroutine(E());
		IEnumerator E()
		{
			bullets.Add(Instantiate(projectile, firePosition.position, Quaternion.identity).Initialise(projectileSpeed, (player.LookingAt - firePosition.position).normalized, player, Color.green, false));
			yield return new WaitForSeconds(fireDelay);
			crtDelay = null;
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < bullets.Count; i++)
		{
			if (bullets[i] != null) Destroy(bullets[i].gameObject);
		}
		bullets.Clear();
	}
}
