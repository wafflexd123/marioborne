using System.Collections;
using UnityEngine;

public class BodySwapLauncher : MonoBehaviourPlus, IPlayerPower
{
    public Bullet projectile;
    public float projectileSpeed, fireDelay;
    public Transform firePosition;
    Player player;
    Coroutine crtDelay;

    public bool CanDisable => true;

    private void Awake()
    {
        if (!FindComponent(transform, out player))
        {
            Debug.LogError("Bodyswapper could not find player!", this);
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Ability")) Shoot();
    }

    public void Shoot()
    {
        if (crtDelay == null) crtDelay = StartCoroutine(E());
        IEnumerator E()
        {
            Instantiate(projectile, firePosition.position, Quaternion.identity).Initialise(projectileSpeed, (player.LookingAt - firePosition.position).normalized, player, Color.green);
            yield return new WaitForSeconds(fireDelay);
            crtDelay = null;
        }
    }
}
