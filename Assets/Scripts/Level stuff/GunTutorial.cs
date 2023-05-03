using System.Collections;
using UnityEngine;

public class GunTutorial : Raycastable
{
	public float maxDistance;
	public GameObject tutorialObject;
	[HideInInspector] public WeaponBase weapon;
	GameObject ui;
	Coroutine crtRaycast;

	private void Awake()
	{
		ui = tutorialObject.transform.Find("UI").gameObject;
		weapon = GetComponent<WeaponBase>();
	}

	public void Destroy()
	{
		Destroy(tutorialObject);
		Destroy(this);
	}

	public override void OnRaycast(Player player)
	{
		if (crtRaycast == null && Vector3.Distance(transform.position, player.transform.position) <= maxDistance) crtRaycast = StartCoroutine(WaitForEndRaycast());
		IEnumerator WaitForEndRaycast()
		{
			ui.SetActive(true);
			while (Vector3.Distance(transform.position, player.transform.position) <= maxDistance && FindComponent(player.raycast.transform, out GunTutorial _))//close enough and while looking at this
			{
				ui.transform.LookAt(player.camera.transform);
				yield return null;
			}
			ui.SetActive(false);
			crtRaycast = null;
		}
	}
}
