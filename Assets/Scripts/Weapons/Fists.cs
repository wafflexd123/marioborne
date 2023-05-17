using System.Collections;
using UnityEngine;

public class Fists : MonoBehaviourPlus
{
    float punchTime = 0.3f;
    float deflectTime = 0.3f;
    public float punchDelay;
	public float deflectDelay;
	Coroutine crtPunchDelay, crtDeflectDelay, crtPunchTime, crtDeflectTime;
	public GameObject reflectWindow;
	private Player player;

	private void Awake()
	{
		player = GetComponentInParent<Player>();
	}

	protected void Update()
	{
		if (player.hand.childCount > 0)
		{
			Enable(false);
		}
		else if (FindComponent(player.raycast.transform, out WeaponBase weapon))
		{
			if (Vector3.Distance(player.transform.position, weapon.transform.position) >= player.maxInteractDistance)
			{
				if (Input.GetMouseButtonDown(0)) LeftMouse();
				if (Input.GetMouseButtonDown(1)) RightMouse();
			}
		}
		else
		{
			if (Input.GetMouseButtonDown(0)) LeftMouse();
			if (Input.GetMouseButtonDown(1)) RightMouse();
		}
	}

	protected void LeftMouse()
	{
		if (player.hand.childCount <= 0)
		{
			if (crtDeflectTime == null)
			{
				if (crtPunchDelay == null && player.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
				{
                    if(crtPunchTime == null)
                    {
                        player.model.punching = true;
                        Enable(true);
                        crtPunchTime = StartCoroutine(Anim());

                        IEnumerator Anim()
                        {
                            yield return new WaitForSeconds(punchTime);
                            Enable(false);
                            player.model.punching = false;
                            crtPunchDelay = StartCoroutine(Delay());
                            crtPunchTime = null;
                        }
                    }
				}

                IEnumerator Delay()
                {
                    yield return new WaitForSeconds(punchDelay);
                    crtPunchDelay = null;
                }
			}
		}
	}

	protected void RightMouse() //handles deflection
	{
		if (player.hand.childCount <= 0)
		{
			if (crtPunchTime == null)
			{
				if (crtDeflectDelay == null && player.LookingAt != Vector3.negativeInfinity)
				{
                    if(crtDeflectTime == null)
                    {
                        player.model.deflect = true;
                        crtDeflectTime = StartCoroutine(Anim());

                        IEnumerator Anim()
                        {
                            reflectWindow.SetActive(true);
                            yield return new WaitForSeconds(deflectTime);
                            reflectWindow.SetActive(false);
                            player.model.deflect = false;
                            crtDeflectDelay = StartCoroutine(Delay());
                            crtDeflectTime = null;
                        }
                    }
				}

                IEnumerator Delay()
                {
                    yield return new WaitForSeconds(deflectDelay);
                    crtDeflectDelay = null;
                }
			}
		}
	}

	private void Enable(bool enable)
	{
		if (enable) GetComponent<BoxCollider>().enabled = true;
		else GetComponent<BoxCollider>().enabled = false;
	}

	void OnTriggerEnter(Collider collider)
	{
		if (FindComponent(collider.transform, out Enemy enemy))
		{
			enemy.Kill();
		}
	}
}