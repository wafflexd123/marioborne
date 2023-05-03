using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorButton : Raycastable
{
	public ElevatorDoor doors;

	public override void OnRaycast(Player player)
	{
		if (Input.GetButtonDown("Interact"))
		{
			doors.Toggle();
		}
	}
}
