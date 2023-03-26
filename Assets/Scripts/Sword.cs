using System.Collections;
using UnityEngine;

public class Sword : WeaponBase
{
	public Position guardPos;
	public float guardSpeed;
	Coroutine crtGuard;

	protected override void OnPickup()
	{
		StartCoroutine(CheckDrop());
		if (crtGuard == null) crtGuard = StartCoroutine(Guard());
		IEnumerator Guard()
		{
			do
			{
				wielder.GetAxisDown("Mouse", out float value);
				if (value > 0)//if right mouse is held
				{
					if (transform.localPosition != guardPos.coords)
					{
						transform.localEulerAngles = guardPos.eulers;//temp
						transform.localPosition = Vector3.MoveTowards(transform.localPosition, guardPos.coords, pickupSpeed * Time.deltaTime);
					}
				}
				else if (transform.localPosition != handPosition.coords)
				{
					transform.localEulerAngles = handPosition.eulers;//temp
					transform.localPosition = Vector3.MoveTowards(transform.localPosition, handPosition.coords, pickupSpeed * Time.deltaTime);
				}
				yield return null;

			} while (BeingHeld());

			crtGuard = null;
		}
	}
}
