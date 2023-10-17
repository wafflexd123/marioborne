using UnityEngine;

public class BodySwapBullet : Bullet
{
    public float teleportSpeed;

    protected override void OnCollisionEnter(Collision collision)
    {
        if (FindComponent(collision.transform, out Humanoid enemy) && enemy != shooter)
        {
            ((Player)shooter).TeleportToEnemy(enemy, teleportSpeed);
        }
        Destroy(gameObject);
    }
}
