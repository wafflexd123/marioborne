using UnityEngine;

public class BodySwapBullet : Bullet
{
    public float teleportSpeed, maxTeleportTime;

    protected override void OnCollisionEnter(Collision collision)
    {
        if (FindComponent(collision.transform, out Humanoid enemy) && enemy != shooter)
        {
            ((Player)shooter).TeleportToEnemy(enemy, teleportSpeed, maxTeleportTime);
        }
        Destroy(gameObject);
    }
}
