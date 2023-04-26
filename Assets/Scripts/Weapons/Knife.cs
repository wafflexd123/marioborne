using System.Collections;
using UnityEngine;

public class Knife : WeaponBase
{
	public float hitDelay;
	Coroutine crtDelay;

	protected override void LeftMouse()
	{
		if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
		{
			crtDelay = StartCoroutine(Delay());
		}

		IEnumerator Delay()
		{
			Debug.Log("you would've died!");
			yield return new WaitForSeconds(hitDelay);
			crtDelay = null;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (FindComponent(collision.transform, out Humanoid humanoid) && humanoid != wielder)
		{

		}
	}
}
