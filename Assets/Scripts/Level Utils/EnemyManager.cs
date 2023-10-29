using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : UnityEventHelper
{
	[Tooltip("Amount of enemies that can be alive when onAllEnemiesDead is called")] public int buffer;
	public UnityEvent onAllEnemiesDead;
	int deadEnemies;

	public void RegisterDeath()
	{
		deadEnemies++;
		if (deadEnemies == transform.childCount - 1 - buffer)
		{
			onAllEnemiesDead.Invoke();
			Destroy(this);
		}
	}

	public void DeregisterDeath()
	{
		deadEnemies--;
		if (deadEnemies < 0) Debug.LogWarning("More deaths have been un-recorded than there are enemies to die in the first place, fix this", this);
	}
}
