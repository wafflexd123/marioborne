using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySwapLauncher : MonoBehaviourPlus
{
	public Bullet projectile;
	public float projectileSpeed, fireDelay;
	public Transform firePosition;
	Player player;
	Coroutine crtDelay;

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
			Instantiate(projectile, firePosition.position, Quaternion.identity).Initialise(projectileSpeed, (player.LookingAt - firePosition.position).normalized, player, Color.green);
			yield return new WaitForSeconds(fireDelay);
			crtDelay = null;
		}
	}
}
