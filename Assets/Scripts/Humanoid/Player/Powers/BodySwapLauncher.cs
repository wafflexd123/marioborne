using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySwapLauncher : MonoBehaviourPlus, IPlayerPower
{
	public GameObject projectile;
	public float projectileSpeed, fireDelay;
	public Transform firePosition;
	Player player;
	Coroutine crtDelay;
	List<Bullet> bullets = new List<Bullet>();

    [Header("Energy Variables")]
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private int bodySwapEnergyCost = 60;

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
        if (crtDelay == null && playerEnergy.GetEnergy() >= bodySwapEnergyCost)
        {
            crtDelay = StartCoroutine(E());
        }
        else playerEnergy.FlashEnergyText();
        IEnumerator E()
        {
            playerEnergy.DecreaseEnergy(bodySwapEnergyCost);
            GameObject proj = Instantiate(projectile, firePosition.position, Quaternion.Euler((player.LookingAt - firePosition.position).normalized));
            BodySwapBullet bullet = proj.GetComponent<BodySwapBullet>();
            bullet.Fire();
            yield return new WaitForSeconds(fireDelay);
            crtDelay = null;
        }
    }

    private void OnDisable()
	{
		for (int i = 0; i < bullets.Count; i++)
		{
			if (bullets[i] != null) Destroy(bullets[i].gameObject);
		}
		bullets.Clear();
	}

	public void OnWeaponPickup()
	{
	}
}
