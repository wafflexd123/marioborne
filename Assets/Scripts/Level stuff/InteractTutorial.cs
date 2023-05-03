using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractTutorial : Raycastable
{
	public GameObject ui;
	Coroutine crtRaycast;

	public override void OnRaycast(Player player)
	{
		if (crtRaycast == null) crtRaycast = StartCoroutine(WaitForEndRaycast());
		IEnumerator WaitForEndRaycast()
		{
			ui.SetActive(true);
			while (FindComponent(player.raycast.transform, out InteractTutorial _))//while looking at this
			{
				yield return null;
			}
			ui.SetActive(false);
			crtRaycast = null;
		}
	}
}
