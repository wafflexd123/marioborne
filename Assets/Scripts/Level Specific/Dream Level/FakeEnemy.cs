using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FakeEnemy : UnityEventHelper, IAttackReceiver
{
	public UnityEvent onHit;

	public void ReceiveAttack(MonoBehaviour attacker, MonoBehaviour weapon, DeathType deathType, Collision collision)
	{
		Destroy(weapon.gameObject);
		Destroy(gameObject);
		onHit.Invoke();
	}
}
