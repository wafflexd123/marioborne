using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionForceManager : MonoBehaviour
{
	public bool explodeOnEnable;
	List<RandomExplosionForce> children = new List<RandomExplosionForce>();

	private void Awake()
	{
		foreach (Transform child in transform) children.Add(child.GetComponent<RandomExplosionForce>());

		if (explodeOnEnable) Explode();
	}

	public void Explode()
	{
		foreach (RandomExplosionForce item in children)
		{
			item.ApplyForce();
		}
	}
}
