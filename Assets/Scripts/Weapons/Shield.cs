using System.Collections;
using UnityEngine;

public class Shield : MonoBehaviour, IBulletReceiver
{
    public void OnBulletHit(Collision collision, Bullet bullet)
    {
        Destroy(bullet);
    }
}
