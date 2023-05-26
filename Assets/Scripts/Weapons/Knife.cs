using System.Collections;
using UnityEngine;

public class Knife : WeaponBase
{
    public float hitDelay, hitTime = 0.3f;
    Coroutine crtDelay;
    bool _isFiring;

    public override bool IsFiring => _isFiring;

    protected void Swing()
    {
        if (crtDelay == null) crtDelay = StartCoroutine(Delay());

        IEnumerator Delay()
        {
            wielder.model.slash = true;
            _isFiring = true;
            yield return new WaitForSeconds(hitTime);
            _isFiring = false;
            if (wielder) wielder.model.slash = false;
            yield return new WaitForSeconds(hitDelay);
            crtDelay = null;
        }
    }

    protected override void OnPickup()
    {
        base.OnPickup();
        wielder.model.holdingMelee = true;
        GetComponent<Collider>().isTrigger = true;
    }

    protected override void OnDrop()
    {
        base.OnDrop();
        GetComponent<Collider>().isTrigger = false;
        wielder.model.holdingMelee = false;
    }

    protected override void LeftMouse()
    {
        if (wielder is not Player || !((Player)wielder).fists.IsFiring) Swing();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (crtDelay != null && wielder != null)
        {
            if (FindComponent(collider.transform, out Humanoid enemy))
            {
                enemy.Kill(DeathType.Melee);
            }
        }
    }
}

//protected override void RightMouse() //handles deflection while holding weapon
//{
//    if(wielder is Player)
//    {
//        if(crtDelay == null)
//        {
//            if (wielder.crtDeflectDelay == null && wielder.LookingAt != Vector3.negativeInfinity)
//            {
//                if (wielder.crtDeflectTime == null)
//                {
//                    wielder.model.deflect = true;
//                    wielder.crtDeflectTime = StartCoroutine(Anim());

//                    IEnumerator Anim()
//                    {
//                        wielder.deflectWindow.SetActive(true);
//                        yield return new WaitForSeconds(wielder.GetComponent<Player>().deflectTime);
//                        wielder.deflectWindow.SetActive(false);
//                        wielder.model.deflect = false;
//                        wielder.crtDeflectDelay = StartCoroutine(Delay());
//                        wielder.crtDeflectTime = null;
//                    }
//                }
//            }

//            IEnumerator Delay()
//            {
//                //yield return new WaitForSeconds(wielder.deflectDelay);
//                float timer = 0;
//                deflectPercent.gameObject.SetActive(true);
//                while (timer < wielder.GetComponent<Player>().deflectDelay)
//                {
//                    timer += Time.fixedDeltaTime;
//                    deflectPercent.fillAmount = timer / wielder.GetComponent<Player>().deflectDelay;
//                    yield return new WaitForFixedUpdate();
//                }
//                deflectPercent.gameObject.SetActive(false);
//                wielder.crtDeflectDelay = null;
//            }
//        }
//    }
