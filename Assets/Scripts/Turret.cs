using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
	public float sightRadius, fireDelay, bulletSpeed;
	public bool fireOnFirstFrame;
	public Bullet bulletPrefab;
	public Transform firePosition;
	float timer;

	private void Start()
	{
		if (fireOnFirstFrame) timer = fireDelay;
	}

	void Update()
	{
		timer += Time.deltaTime;
		if (timer > fireDelay)
		{
			timer = 0;
			Collider[] ray = Physics.OverlapSphere(transform.position, sightRadius, 1 << 3);
			if (ray.Length > 0 && ray[0] != null) Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, (firePosition.position - ray[0].transform.position).normalized);
		}
	}
}
