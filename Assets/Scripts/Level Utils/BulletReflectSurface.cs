using UnityEngine;

public class BulletReflectSurface : MonoBehaviourPlus, IAttackReceiver
{
    public bool enableReflect;
    public Color bulletColor;

    public void ReceiveAttack(MonoBehaviour attacker, MonoBehaviour weapon, DeathType deathType, Collision collision)
    {
        if (weapon is Bullet bullet)
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
}
