using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapObject : MonoBehaviourPlus
{
	public Transform leapDirection;
	public float leapForce;
	public Collider[] collisionColliders;
	bool playerInTrigger;

	public bool CanLeap(Transform lookDirection)
	{
		return playerInTrigger; /*&& Mathf.Abs(leapDirection.eulerAngles.y - lookDirection.eulerAngles.y) <= maxLookAngleDifference;*///player is in trigger and looking in the right direction
	}

	public Vector3 GetLeapForce(float mass)
	{
		for (int i = 0; i < collisionColliders.Length; i++) collisionColliders[i].isTrigger = true;//stop player from colliding with box while leaping
		Debug.Log(leapForce * mass * leapDirection.forward);
		return leapForce * mass * leapDirection.forward;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (FindComponent(other.transform, out PlayerMovement player))
		{
			if (player.closestLeapObject == null) player.closestLeapObject = this;
			playerInTrigger = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (FindComponent(other.transform, out PlayerMovement player))
		{
			if (player.closestLeapObject == this) player.closestLeapObject = null;
			playerInTrigger = false;
			for (int i = 0; i < collisionColliders.Length; i++) collisionColliders[i].isTrigger = false;
		}
	}
}


