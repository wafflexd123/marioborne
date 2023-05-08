using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchLedge : MonoBehaviourPlus
{
	public Transform bodyPos;

	private void OnTriggerEnter(Collider other)
	{
		if (FindComponent(other.transform, out PlayerMovement player))
		{
			player.closestCatchLedge = this;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (FindComponent(other.transform, out PlayerMovement player))
		{
			if (player.closestCatchLedge == this) player.closestCatchLedge = null;
		}
	}
}
