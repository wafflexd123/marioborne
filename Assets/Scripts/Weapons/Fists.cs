using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fists : MonoBehaviourPlus
{
    float punchTime, deflectTime, punchDelay, deflectDelay;
	Coroutine crtPunchDelay, crtDeflectDelay, crtPunchTime, crtDeflectTime;
	public GameObject reflectWindow;
	private Player player;

    GameObject ui;
    Image deflectPercent;

    private void Awake()
	{
		player = GetComponentInParent<Player>();
	}

    private void Start()
    {
        punchTime = player.punchTime;
        deflectTime = player.deflectTime;
        punchDelay = player.punchDelay;
        deflectDelay = player.deflectDelay;

        ui = transform.parent.parent.Find("UI").gameObject;
        deflectPercent = ui.transform.Find("Deflect").GetComponent<Image>();
        ui.SetActive(true);
    }

    protected void Update()
	{
        if (!player.hasDied)
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
                    //yield return new WaitForSeconds(deflectDelay);
                    float timer = 0;
                    deflectPercent.gameObject.SetActive(true);
                    while (timer < deflectDelay)
                    {
                        timer += Time.fixedDeltaTime;
                        deflectPercent.fillAmount = timer / deflectDelay;
                        yield return new WaitForFixedUpdate();
                    }
                    deflectPercent.gameObject.SetActive(false);
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