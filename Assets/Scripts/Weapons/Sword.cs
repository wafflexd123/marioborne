using System.Collections;
using UnityEngine;

public class Sword : WeaponBase
{
    public float hitDelay;
    Coroutine crtDelay;
    private bool holding = false;

    protected override void OnPickup()
    {
        base.OnPickup();
        wielder.model.holdingMelee = true;
        holding = true;
    }

    protected override void OnDrop()
    {
        base.OnDrop();
        wielder.model.holdingMelee = false;
        holding = false;
    }

    protected override void Update()
    {
        if (rigidbody != null && wielder != null)
        {
            if (wielder.GetComponent<Player>()) rigidbody.excludeLayers = LayerMask.GetMask("Player");
            else if (wielder.GetComponent<Enemy>()) rigidbody.excludeLayers = LayerMask.GetMask("Enemy");
        }

        if (holding) transform.position = wielder.hand.position;
    }

    protected override void LeftMouse()
    {
        if (crtDelay == null && wielder.LookingAt != Vector3.negativeInfinity)//if not waiting for fireDelay && wielder is looking at something
        {
            wielder.model.attacking = true;
            crtDelay = StartCoroutine(Delay());
        }

        IEnumerator Delay()
        {
            EnableRigidbody(true);
            rigidbody.isKinematic = true;
            yield return new WaitForSeconds(hitDelay);
            if (wielder != null)
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
        if (crtDelay != null && FindComponent(collision.collider.transform, out Humanoid humanoid))
        {
            humanoid.Kill();
        }
    }
}