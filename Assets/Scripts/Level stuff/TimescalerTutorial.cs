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
					player.StartCoroutine(LerpToPos(scaler.transform, new Position(player.hand), 5f, ()=> Destroy(transform.parent.gameObject)));
					yield break;
				}
				yield return null;
			} 
			ui.SetActive(false);
			crtRaycast = null;
		}
	}
}
