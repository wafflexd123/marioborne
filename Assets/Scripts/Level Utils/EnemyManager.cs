using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : UnityEventHelper
{
	public UnityEvent onAllEnemiesDead;
	int deadEnemies;

	public void RegisterDeath()
	{
		deadEnemies++;
		if (deadEnemies >= transform.childCount)
		{
			onAllEnemiesDead.Invoke();
			if (deadEnemies > transform.childCount) Debug.LogWarning("More deaths have been recorded than there are enemies to die in the first place, fix this", this);
		}
	}

	public void DeregisterDeath()
	{
		deadEnemies--;
		if (deadEnemies < 0) Debug.LogWarning("More deaths have been un-recorded than there are enemies to die in the first place, fix this", this);
	}
}
