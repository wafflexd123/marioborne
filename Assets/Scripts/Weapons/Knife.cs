using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Knife : WeaponBase
{
    float hitTime = 0.3f;
    public float hitDelay;
    Coroutine crtDelay, crtHit;
    private Player player;

    GameObject ui;
    Image deflectPercent;

    protected override void Start()
    {
        base.Start();
        ui = transform.Find("UI").gameObject;
        deflectPercent = ui.transform.Find("Deflect").GetComponent<Image>();
    }

    protected override void OnPickup()
    {
        base.OnPickup();
        wielder.model.holdingMelee = true;
        GetComponent<Collider>().isTrigger = true;
        if (wielder is Player)
        {
            player = wielder.GetComponent<Player>();
            ui.SetActive(true);
        }
    }

    protected override void OnDrop()
    {
        base.OnDrop();
        GetComponent<Collider>().isTrigger = false;
        wielder.model.holdingMelee = false;
    }

    protected override void LeftMouse()
    {
        if (wielder.crtDeflectTime == null)
        {
            Swing();
        }
    }

    protected override void RightMouse() //handles deflection while holding weapon
    {
        if(wielder is Player)
        {
            if(crtDelay == null)
            {
                if (wielder.crtDeflectDelay == null && wielder.LookingAt != Vector3.negativeInfinity)
                {
                    if (wielder.crtDeflectTime == null)
                    {
                        wielder.model.deflect = true;
                        wielder.crtDeflectTime = StartCoroutine(Anim());

                        IEnumerator Anim()
                        {
                            wielder.deflectWindow.SetActive(true);
                            yield return new WaitForSeconds(wielder.GetComponent<Player>().deflectTime);
                            wielder.deflectWindow.SetActive(false);
                            wielder.model.deflect = false;
                            wielder.crtDeflectDelay = StartCoroutine(Delay());
                            wielder.crtDeflectTime = null;
                        }
                    }
                }

                IEnumerator Delay()
                {
                    //yield return new WaitForSeconds(wielder.deflectDelay);
                    float timer = 0;
                    deflectPercent.gameObject.SetActive(true);
                    while (timer < wielder.GetComponent<Player>().deflectDelay)
                    {
                        timer += Time.fixedDeltaTime;
                        deflectPercent.fillAmount = timer / wielder.GetComponent<Player>().deflectDelay;
                        yield return new WaitForFixedUpdate();
                    }
                    deflectPercent.gameObject.SetActive(false);
                    wielder.crtDeflectDelay = null;
                }
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (crtDelay != null && wielder != null)
        {
            if (wielder.GetComponent<Player>())
            {
                if (FindComponent(collider.transform, out Enemy enemy))
                {
                    enemy.Kill(DeathType.Melee);
                }
            }
            else if (wielder.GetComponent<Enemy>())
            {
                if (FindComponent(collider.transform, out Player player))
                {
                    player.Kill(DeathType.Melee);
                }
            }
        }
    }

    protected void Swing()
    {
        if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
        {
            if (crtHit == null)
            {
                wielder.model.melee = true;
                crtHit = StartCoroutine(Anim());

                IEnumerator Anim()
                {
                    yield return new WaitForSeconds(hitTime);
                    if (wielder) wielder.model.melee = false;
                    crtDelay = StartCoroutine(Delay());
                    crtHit = null;
                }
            }

            IEnumerator Delay()
            {
                yield return new WaitForSeconds(hitDelay);
                crtDelay = null;
            }
        }
    }
}