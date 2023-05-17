using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyPath
{
	public List<Enemy> enemies = new List<Enemy>();
	public Transform path;

	public bool AreDead()
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i] != null) return false;
		}
		return true;
	}

	public void SetPaths()
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i] != null) enemies[i].points = path;
		}
	}
}

[System.Serializable]
public class EnemyPathManager
{
	public List<EnemyPath> enemyPaths = new List<EnemyPath>();
	List<EnemyPath> activeEnemies = new List<EnemyPath>();
	int currentPos = 0;

	public EnemyPathManager(Transform root)
	{
		foreach (Transform enemiesAndPaths in root)
		{
			enemyPaths.Add(new EnemyPath());
			foreach (Transform item in enemiesAndPaths)
			{
				if (item.TryGetComponent(out Enemy enemy)) enemyPaths[^1].enemies.Add(enemy);
				else enemyPaths[^1].path = item;
			}
		}
	}

	public bool ActiveEnemiesAreDead()
	{
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if (activeEnemies[i].AreDead()) activeEnemies.RemoveAt(i--);
			else return false;
		}
		return true;
	}

	public void SetNextEnemyPath()
	{
		enemyPaths[currentPos].SetPaths();
		activeEnemies.Add(enemyPaths[currentPos]);
		currentPos++;
	}
}