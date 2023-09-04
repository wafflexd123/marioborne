using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RandomExplosionForce : MonoBehaviour
{
	public bool applyOnEnable;
	public float minForce, maxForce, radius;

	private void OnEnable()
	{
		if (applyOnEnable) ApplyForce();
	}

	public void ApplyForce()
	{
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.isKinematic = false;
		rb.AddExplosionForce(Random.Range(minForce, maxForce), transform.parent.position, radius);
	}
}
