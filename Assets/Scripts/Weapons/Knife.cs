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
        if (wielder.GetComponent<Player>()) player = wielder.GetComponent<Player>();
    }

    protected override void OnDrop()
    {
        base.OnDrop();
        Attack(false);
        wielder.model.holdingMelee = false;
    }

    protected override void Update()
    {
        //if (BeingHeld()) transform.position = wielder.hand.position;
    }

    protected override void LeftMouse()
    {
        if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
        {
            Debug.Log("attacking");
            wielder.model.attacking = true;
            crtDelay = StartCoroutine(Delay());
        }

        IEnumerator Delay()
        {
            EnableRigidbody(true);
            Attack(true);
            yield return new WaitForSeconds(hitDelay);
            if(wielder) wielder.model.attacking = false;
            Attack(false);
            EnableRigidbody(false);
            crtDelay = null;
        }
    }

    protected override void RightMouse()
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

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.transform.name);
        if (crtDelay != null && wielder != null)
        {
            if (wielder.GetComponent<Player>())
            {
                if (FindComponent(collision.collider.transform, out Enemy enemy))
                {
                    enemy.Kill();
                }
            }
            else if (wielder.GetComponent<Enemy>())
            {
                if (FindComponent(collision.collider.transform, out Player player))
                {
                    player.Kill();
                }
            }
        }
    }
}