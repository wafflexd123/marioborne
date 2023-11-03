using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BasicTelekineticObject : MonoBehaviour, ITelekinetic
{
	new Rigidbody rigidbody;
	int layer;

	void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		layer = gameObject.layer;
	}

	public void TelekineticGrab(Telekinesis t)
	{
		rigidbody.isKinematic = false;
		rigidbody.useGravity = false;
		gameObject.layer = 17;
	}

	public void TelekineticRelease()
	{
		rigidbody.useGravity = true;
		gameObject.layer = layer;
	}
}
