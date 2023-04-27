using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnOnPlayerTouch : MonoBehaviourPlus
{
	public DeathType deathType;

	private void OnCollisionEnter(Collision collision)
	{
		if (FindComponent(collision.transform, out Player p))
		{
			p.Kill(deathType);
		}
	}
}
