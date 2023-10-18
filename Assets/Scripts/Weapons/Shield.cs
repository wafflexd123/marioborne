using System.Collections;
using UnityEngine;

public class Shield : MonoBehaviour, IBulletReceiver
{
    public void OnBulletHit(Collision collision, Bullet bullet)
    {
        if (!bullet.penetrates) Destroy(bullet);
        else Destroy(this.gameObject);
    }
}
