using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorButton : Raycastable
{
	public ElevatorDoor doors;
	public GameObject ui;
	public bool interactable = true;
	Coroutine crtRaycast;

	public override void OnRaycast(Player player)
	{
		if (interactable)
		{
			if (Input.GetButtonDown("Interact"))
			{
				doors.Toggle();
			}
			if (ui != null && crtRaycast == null) crtRaycast = StartCoroutine(WaitForEndRaycast(player));
		}
	}

	IEnumerator WaitForEndRaycast(Player player)
	{
		ui.SetActive(true);
		while (FindComponent(player.raycast.transform, out ElevatorButton _))//while looking at this
		{
			yield return null;
		}
		ui.SetActive(false);
		crtRaycast = null;
	}
}
