using UnityEngine;

public class BodySwapBullet : Bullet
{
	public float teleportSpeed;

	protected override void OnCollisionEnter(Collision collision)
	{
		if (FindComponent(collision.transform, out Enemy enemy) && shooter is Player player)
		{
			player.TeleportToEnemy(enemy, teleportSpeed);
		}
		else Destroy(gameObject);
	}
}
