using System.Collections;
using UnityEngine;

public class GunTutorial : Raycastable
{
	public GameObject tutorialObject;
	Coroutine crtRaycast;
	public Enemy[] enemies;
	public Transform path;
	GameObject ui;

	private void Start()
	{
		ui = tutorialObject.transform.Find("UI").gameObject;
	}

	public override void OnRaycast(Player player)
	{
		if (ui != null && crtRaycast == null) crtRaycast = StartCoroutine(WaitForEndRaycast());
		IEnumerator WaitForEndRaycast()
		{
			ui.SetActive(true);
			while (FindComponent(player.raycast.transform, out WeaponBase weapon))//while looking at this
			{
				ui.transform.LookAt(player.camera.transform);
				if (weapon.BeingHeld())
				{
					for (int i = 0; i < enemies.Length; i++)
					{
						enemies[i].points = path;
					}
					Destroy(tutorialObject);
					Destroy(this);
					yield break;
				}
				yield return null;
			}
			ui.SetActive(false);
			crtRaycast = null;
		}
	}
}
