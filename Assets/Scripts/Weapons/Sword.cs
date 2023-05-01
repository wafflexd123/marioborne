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
    public float hitDelay;
    public Transform eyes;
    public float meleeRadius;
    [Range(0, 360)]
    public float meleeAngle;
    public LayerMask targetMask, obstacleMask;

    protected override void OnPickup()
	{
		base.OnPickup();
		reflectWindow = wielder.transform.Find("ReflectWindow").gameObject;
        wielder.model.holdingWeapon = true;
        if (wielder.GetComponent<Player>())
        {
            eyes = wielder.GetComponentInParent<Player>().camera.transform;
            targetMask = LayerMask.GetMask("Enemy", "Bullet");
        }
        else
        {
            eyes = wielder.GetComponentInParent<Enemy>().head;
            targetMask = LayerMask.GetMask("Player", "Bullet");
        }
    }

    protected override void LeftMouse()
    {
        if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
        {
            Collider collider = MeleeCheck();
            if (collider != null)
            {
                Debug.Log(collider.name);
                if (collider.transform.gameObject.GetComponentInParent<Player>()) collider.transform.gameObject.GetComponentInParent<Player>().Kill(DeathType.General);
                else if (collider.transform.gameObject.GetComponentInParent<Enemy>()) collider.transform.gameObject.GetComponentInParent<Enemy>().Kill(DeathType.General);
            }
            crtDelay = StartCoroutine(Delay());
        }

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(hitDelay);
            crtDelay = null;
        }
    }

    Collider MeleeCheck()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(eyes.position, meleeRadius, targetMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Debug.Log(targetsInViewRadius[i].transform.parent.name);
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = target.position + new Vector3(0, 1.5f) - eyes.position;
            //target is within fow?
            //Debug.DrawLine(eyes.position, target.position + new Vector3(0, 1.5f));
            if (Vector3.Angle(eyes.forward, dirToTarget) < meleeAngle / 2)
            {
                //Debug.Log("within angle");
                if (!Physics.Linecast(eyes.position, target.position + new Vector3(0, 1.5f), obstacleMask))
                {
                    return targetsInViewRadius[i];
                }
            }
        }
        return null;
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
