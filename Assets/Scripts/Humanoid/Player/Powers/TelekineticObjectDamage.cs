using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelekineticObjectDamage : MonoBehaviour
{
    private Rigidbody rb;
    private bool ready = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(GetReady());
    }

    private IEnumerator GetReady()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        ready = true;
    }

    void FixedUpdate()
    {
        if (!ready) return;

        float curSpeed = rb.velocity.magnitude;
        if (curSpeed < Telekinesis.KillingSpeedThreshold)
        {
            //print("went to slow");
            Destroy(this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("Telekinesis box collided with object: " + collision.gameObject.name + ",\t going at speed: " + rb.velocity.magnitude);
        if (collision.gameObject.layer == 11) // Enemy
        {
            print("Collided with enemy layer");
            Humanoid humanoid = collision.gameObject.GetComponentInParent<Humanoid>();
            if (humanoid != null)
            {
                humanoid.Kill();
                Destroy(this);
            }
        }
    }
}
