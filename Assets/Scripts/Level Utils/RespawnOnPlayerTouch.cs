using UnityEngine;

public class RespawnOnPlayerTouch : MonoBehaviourPlus
{
    public DeathType deathType;

    private void OnCollisionEnter(Collision collision)
    {
        if (FindComponent(collision.transform, out Player p))
        {
            p.Kill(deathType);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (FindComponent(collision.transform, out Player p))
        {
            p.Kill(deathType);
        }
    }
}
