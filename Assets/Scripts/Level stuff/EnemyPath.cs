using UnityEngine;

[System.Serializable]
public class EnemyPath
{
	public Enemy[] enemies;
	public Transform path;

	public bool AreDead()
	{
		for (int i = 0; i < enemies.Length; i++)
		{
			if (enemies[i] != null) return false;
		}
		return true;
	}

	public void SetPaths()
	{
		for (int i = 0; i < enemies.Length; i++)
		{
			enemies[i].points = path;
		}
	}
}