using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviourPlus
{
	public Position handPosition;
	public float pickupSpeed, dropForce;
	public Collider[] colliders;
	protected Humanoid wielder;
	protected new Rigidbody rigidbody;
	protected RigidbodyStore rigidbodyStore;

	protected virtual void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		FindComponent(transform, out wielder);//set wielder and subsequent BeingHeld() value
		if (BeingHeld())
		{
			EnableRigidbody(false);
			OnPickup();//simulate being picked up if already held
		}
	}

	public virtual bool Pickup(Humanoid humanoid)
	{
		if (!BeingHeld())
		{
			wielder = humanoid;
			transform.parent = humanoid.hand;
			EnableRigidbody(false);
			StartCoroutine(LerpToPos(transform, handPosition, pickupSpeed, () => OnPickup()));
			return true;
		}
		return false;
	}

	protected virtual void OnPickup()
	{
		StartCoroutine(CheckDrop());
		StartCoroutine(CheckMouseClick());
	}

	public virtual void Drop()
	{
		wielder = null;
		transform.parent = null;
		EnableRigidbody(true);
		rigidbody.AddRelativeForce(Vector3.forward * dropForce, ForceMode.Impulse);
	}

	protected virtual void LeftMouse()
	{

	}

	protected virtual void RightMouse()
	{

	}

	protected virtual bool BeingHeld()
	{
		return wielder != null;
	}

	protected IEnumerator CheckMouseClick()
	{
		do
		{
			if (wielder.GetAxisDown("Mouse", out float mouseButton))
			{
				if (mouseButton < 0) LeftMouse();
				else RightMouse();
			}
			yield return null;
		} while (BeingHeld());
	}

	protected IEnumerator CheckDrop()
	{
		do
		{
			if (wielder.GetAxis("Drop") != 0) Drop();
			yield return null;
		} while (BeingHeld());
	}

	/// <summary>
	/// When an object with a rigidbody is set as a child of another object with a rigidbody, it gets buggy. This destroys the rigidbody and saves its data for later use.
	/// </summary>
	public void EnableRigidbody(bool enable)
	{
		if (enable)
		{
			if ((rigidbody = GetComponent<Rigidbody>()) == null) rigidbody = gameObject.AddComponent<Rigidbody>();//if no rigidbody exists yet
			if (rigidbodyStore != null) rigidbodyStore.Apply(rigidbody);//if a rigidbody has been stored previously
			for (int i = 0; i < colliders.Length; i++) colliders[i].isTrigger = false;
		}
		else if (rigidbody != null)
		{
			rigidbodyStore = new RigidbodyStore(rigidbody);//store rigidbody data
			Destroy(rigidbody);
			for (int i = 0; i < colliders.Length; i++) colliders[i].isTrigger = true;
		}
	}
}
