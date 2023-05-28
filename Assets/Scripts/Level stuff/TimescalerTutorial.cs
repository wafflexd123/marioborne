using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimescalerTutorial : Raycastable
{
	public GameObject ui;
	Coroutine crtRaycast;
	public CarparkLevelManager carparkLevelManager;

	public override void OnRaycast(Player player)
	{
		if (ui != null && crtRaycast == null) crtRaycast = StartCoroutine(WaitForEndRaycast());
		IEnumerator WaitForEndRaycast()
		{
			ui.SetActive(true);
			while (FindComponent(player.raycast.transform, out TimescalerTutorial scaler))//while looking at this
			{
				ui.transform.LookAt(player.camera.transform);
				if (Input.GetMouseButtonDown(0))//if clicked on watch
				{
					carparkLevelManager.timeScalerEnable = true;
					Destroy(ui);
					transform.parent = player.transform;
					StartCoroutine(LerpToPos(transform, new Position(player.hand), 1f, ()=> Destroy(gameObject), .1f));
					yield break;
				}
				yield return null;
			} 
			ui.SetActive(false);
			crtRaycast = null;
		}
	}
}
