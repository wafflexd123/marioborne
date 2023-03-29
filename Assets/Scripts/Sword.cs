using System.Collections;
using UnityEngine;

public class Sword : WeaponBase
{
	public Position guardPos;
	public float guardSpeed;
	Coroutine crtGuard;
    public GameObject reflectWindow;

	protected override void RightMouse()
	{
		if (crtGuard == null) crtGuard = StartCoroutine(Guard());
		IEnumerator Guard()
		{
			while (BeingHeld())//make sure to end coroutine if weapon is dropped
			{
				if (wielder.GetAxis("Mouse") > 0)//if right mouse is still being held
				{
					if (transform.localPosition != guardPos.coords)
					{
						transform.localEulerAngles = guardPos.eulers;//temp
						transform.localPosition = Vector3.MoveTowards(transform.localPosition, guardPos.coords, pickupSpeed * Time.deltaTime);
					}
                    reflectWindow.SetActive(true);
				}
				else if (transform.localPosition != handPosition.coords)
				{
					transform.localEulerAngles = handPosition.eulers;//temp
					transform.localPosition = Vector3.MoveTowards(transform.localPosition, handPosition.coords, pickupSpeed * Time.deltaTime);
                    
                    reflectWindow.SetActive(false);
                }
                else break;//if not pressing mouse and not holstering
				yield return null;
			}
			crtGuard = null;
		}
	}
}
