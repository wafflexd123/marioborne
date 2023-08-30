using UnityEngine;

public class BulletReflectSurface : MonoBehaviourPlus, IBulletReceiver
{
    public bool enableReflect;
    public Color bulletColor;

    public void OnBulletHit(Collision collision, Bullet bullet)
    {
        if (enableReflect)
        {
            bullet.direction = Vector3.Reflect(bullet.direction, collision.GetContact(0).normal);
            bullet.shooter = this;
            bullet.color = bulletColor;
        }
        else Destroy(bullet.gameObject);
    }
}
