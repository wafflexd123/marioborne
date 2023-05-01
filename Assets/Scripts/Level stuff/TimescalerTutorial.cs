using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimescalerTutorial : Raycastable
{
	public GameObject ui;
	Coroutine crtRaycast;
	public Enemy[] enemies;
	public Transform path;

	public override void OnRaycast(Player player)
	{
		if (ui != null && crtRaycast == null) crtRaycast = StartCoroutine(WaitForEndRaycast());
		IEnumerator WaitForEndRaycast()
		{
			ui.SetActive(true);
			while (FindComponent(player.raycast.transform, out TimeScaler scaler))//while looking at this
			{
				ui.transform.LookAt(player.camera.transform);
				if (Input.GetMouseButtonDown(0))//if clicked on watch
				{
					scaler.transform.SetParent(player.transform);
					player.StartCoroutine(LerpToPos(scaler.transform, new Position(player.hand), 5f));

					for (int i = 0; i < enemies.Length; i++)
					{
						enemies[i].points = path;
					}

					Destroy(gameObject);
					yield break;
				}
				yield return null;
			} 
			ui.SetActive(false);
			crtRaycast = null;
		}
	}
}
