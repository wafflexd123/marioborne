using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FakeEnemy : UnityEventHelper, IBulletReceiver
{
	public UnityEvent onHit;

	public void OnBulletHit(Collision collision, Bullet bullet)
	{
		Destroy(bullet.gameObject);
		Destroy(gameObject);
		onHit.Invoke();
	}
}
