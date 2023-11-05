using UnityEngine;

public class RespawnOnPlayerTouch : MonoBehaviourPlus
{
	public DeathType deathType;
	public bool overrideInvincibility;

	private void OnCollisionEnter(Collision collision)
	{
		if (Time.isRewinding) return;
		if (FindComponent(collision.transform, out Humanoid h))
		{
			if (h is Player p && overrideInvincibility) p.ForceKill(deathType);
			else h.Kill(deathType);
		}
	}

	private void OnTriggerEnter(Collider collision)
	{
		if (Time.isRewinding) return;
		if (FindComponent(collision.transform, out Humanoid h))
		{
			if (h is Player p && overrideInvincibility) p.ForceKill(deathType);
			else h.Kill(deathType);
		}
	}
}
