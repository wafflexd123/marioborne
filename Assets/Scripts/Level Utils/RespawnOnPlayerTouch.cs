using UnityEngine;

public class RespawnOnPlayerTouch : MonoBehaviourPlus
{
	public DeathType deathType;
	public bool overrideInvincibility;

	private void OnCollisionEnter(Collision collision)
	{
		if (Time.isRewinding) return;
		if (FindComponent(collision.transform, out Player p))
		{
			if (overrideInvincibility) p.ForceKill(deathType);
			else p.Kill(deathType);
		}
	}

	private void OnTriggerEnter(Collider collision)
	{
		if (Time.isRewinding) return;
		if (FindComponent(collision.transform, out Player p))
		{
			if (overrideInvincibility) p.ForceKill(deathType);
			else p.Kill(deathType);
		}
	}
}
