using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapObject : MonoBehaviourPlus
{
	public Transform leapDirection;
	public float leapForce, maxLookAngleDifference;
	public Collider collisionCollider;
	bool playerInTrigger;

	public bool CanLeap(Transform lookDirection)
	{
		return playerInTrigger && Mathf.Abs(leapDirection.eulerAngles.y - lookDirection.eulerAngles.y) <= maxLookAngleDifference;//player is in trigger and looking in the right direction
	}

	public Vector3 GetLeapForce(float mass, float currentWalkForceMagnitude)
	{
		collisionCollider.isTrigger = true;//stop player from colliding with box while leaping
		//return Mathf.Lerp(minLeapForce, maxLeapForce, currentWalkForceMagnitude) * mass * leapDirection.forward;
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
			collisionCollider.isTrigger = false;
		}
	}
}


