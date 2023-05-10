using System.Collections;
using UnityEngine;

public class Knife : WeaponBase
{
    public float hitDelay;
    Coroutine crtDelay;
    private Player player;

    protected override void OnPickup()
    {
        base.OnPickup();
        wielder.model.holdingMelee = true;
        GetComponent<Collider>().isTrigger = true;
        if (wielder.GetComponent<Player>()) player = wielder.GetComponent<Player>();
    }

    protected override void OnDrop()
    {
        base.OnDrop();
        GetComponent<Collider>().isTrigger = false;
        wielder.model.holdingMelee = false;
    }

    protected override void LeftMouse()
    {
        if (wielder.GetComponent<Player>())
        {
            if (player.crtDeflectDelay == null)
            {
                Swing();
            }
        }
        else Swing();
    }

    protected override void RightMouse() //handles deflection while holding weapon
    {
        if (wielder.GetComponent<Player>() && crtDelay == null)
        {
            if (player.crtDeflectDelay == null && wielder.LookingAt != Vector3.negativeInfinity)
            {
                wielder.model.deflect = true;
                player.crtDeflectDelay = StartCoroutine(Delay());
            }

            IEnumerator Delay()
            {
                player.deflectWindow.SetActive(true);
                yield return new WaitForSeconds(player.deflectDelay);
                player.deflectWindow.SetActive(false);
                wielder.model.deflect = false;
                player.crtDeflectDelay = null;
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
                    enemy.Kill();
                }
            }
            else if (wielder.GetComponent<Enemy>())
            {
                if (FindComponent(collider.transform, out Player player))
                {
                    player.Kill();
                }
            }
        }
    }

    protected void Swing()
    {
        if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
        {
            wielder.model.melee = true;
            crtDelay = StartCoroutine(Delay());
        }

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(hitDelay);
            if (wielder) wielder.model.melee = false;
            crtDelay = null;
        }
    }
}