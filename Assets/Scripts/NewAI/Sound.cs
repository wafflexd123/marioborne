using UnityEngine;

public static class Sound
{
	public static void MakeSound(Vector3 position, float radius, Humanoid humanoid)
	{
		if (humanoid is Player)
		{
			Collider[] enemiesHeard = Physics.OverlapSphere(position, radius, 1 << 11);
			foreach (Collider enemyHeard in enemiesHeard)//creates an overlap sphere around player, checks if enemies are in it and prompts them to investigate
			{
				if (MonoBehaviourPlus.FindComponent(enemyHeard.transform, out AIController ai)) ai.SoundLocation = position;
			}
		}
	}
}
