using System.Collections;
using UnityEngine;

public class Knife : WeaponBase
{
    public float hitDelay;
    Coroutine crtDelay;

    protected override void OnPickup()
    {
        base.OnPickup();
        wielder.model.holdingMelee = true;
    }

    protected override void OnDrop()
    {
        base.OnDrop();
        wielder.model.holdingMelee = false;
    }

    protected override void Update()
    {
        if (rigidbody != null && wielder != null)
        {
            //if (wielder.GetComponent<Player>()) rigidbody.excludeLayers = LayerMask.GetMask("Player");
            //if (wielder.GetComponent<Enemy>()) rigidbody.excludeLayers = LayerMask.GetMask("Enemy");
        }

        if (BeingHeld()) transform.position = wielder.hand.position;
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
            yield return new WaitForSeconds(hitDelay);
            if(wielder != null)
            {
                wielder.model.attacking = false;
            }
            EnableRigidbody(false);
            crtDelay = null;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.transform.name);
        if (crtDelay != null)
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