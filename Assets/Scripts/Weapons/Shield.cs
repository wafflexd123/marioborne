using System.Collections;
using UnityEngine;

public class Shield : MonoBehaviour, IAttackReceiver
{
	public void ReceiveAttack(MonoBehaviour attacker, MonoBehaviour weapon, DeathType deathType, Collision collision)
	{
		if (weapon is Bullet bullet && bullet.penetrates) Destroy(gameObject);
	}
}
