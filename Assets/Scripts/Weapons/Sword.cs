using System.Collections;
using UnityEngine;

public class Sword : WeaponBase
{
	public Position guardPos;
	public float guardSpeed;
	Coroutine crtGuard;
    Coroutine crtDelay;
	GameObject reflectWindow;
    public float reflectDelay;

	protected override void OnPickup()
	{
		base.OnPickup();
		reflectWindow = wielder.transform.Find("ReflectWindow").gameObject;
	}

    protected override void RightMouse()
    {
        if (crtDelay == null) crtGuard = StartCoroutine(Guard());
        IEnumerator Guard()
        {
            if (transform.localPosition != guardPos.coords)
            {
                transform.localEulerAngles = guardPos.eulers;//temp
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, guardPos.coords, pickupSpeed * Time.deltaTime);
            }

            reflectWindow.SetActive(true);
            yield return new WaitForSeconds(0.75f);

            if (transform.localPosition != handPosition.coords)
            {
                transform.localEulerAngles = handPosition.eulers;//temp
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, handPosition.coords, pickupSpeed * Time.deltaTime);

                reflectWindow.SetActive(false);
            }
            crtGuard = null;
            crtDelay = StartCoroutine(Delay());
            yield return null;
        }
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(reflectDelay);
        crtDelay = null;
    }
}
