using UnityEngine;

public class BodySwapBullet : Bullet
{
    public float teleportSpeed;

    protected override void OnCollisionEnter(Collision collision)
    {
        if (FindComponent(collision.transform, out Humanoid enemy) && enemy is not Player && shooter is Player player)
        {
            player.TeleportToEnemy(enemy, teleportSpeed);
        }
        Destroy(gameObject);
    }
}
