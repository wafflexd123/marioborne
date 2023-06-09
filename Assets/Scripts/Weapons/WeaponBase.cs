using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public abstract class WeaponBase : MonoBehaviourPlus
{
	public Position handPosition;
	public float pickupSpeed, dropForce;
	public Collider[] colliders;
	protected Humanoid wielder;
	protected new Rigidbody rigidbody;
	protected RigidbodyStore rigidbodyStore;
	bool isMoving;
	List<UniInput.InputAction> inputActions = new List<UniInput.InputAction>();
	Action onDrop;

	public abstract bool IsFiring { get; }

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

	protected virtual void Update()
	{
		if (rigidbody != null && rigidbody.velocity != Vector3.zero) isMoving = true;
		else isMoving = false;
	}

	public virtual bool Pickup(Humanoid humanoid, Action onDrop = null)
	{
		if (!BeingHeld())
		{
			wielder = humanoid;
			transform.parent = humanoid.hand;
			this.onDrop = onDrop;
			EnableRigidbody(false);
			StartCoroutine(LerpToPos(transform, handPosition, pickupSpeed, () => OnPickup()));
			return true;
		}
		return false;
	}

	public virtual void Drop()
	{
		OnDrop();
		onDrop?.Invoke();
		onDrop = null;
		wielder = null;
		transform.parent = null;
		EnableRigidbody(true);
		rigidbody.AddRelativeForce(Vector3.forward * dropForce, ForceMode.Impulse);
	}

	/// <summary>
	/// Called when item is picked up by a humanoid. Set input listeners here.
	/// </summary>
	protected virtual void OnPickup()
	{
		inputActions.Add(wielder.input.AddListener("Mouse", InputType.OnPress, (float direction) =>
		{
			if (direction < 0) LeftMouse();
			else RightMouse();
		}));
		inputActions.Add(wielder.input.AddListener("Drop", InputType.OnPress, (float _) => Drop()));
	}

	/// <summary>
	/// Called when item is dropped by a humanoid. Remove input listeners here.
	/// </summary>
	protected virtual void OnDrop()
	{
		foreach (UniInput.InputAction action in inputActions) action.RemoveListener();
		inputActions.Clear();
	}

	protected virtual void LeftMouse()
	{

	}

	protected virtual void RightMouse()
	{

	}

	public virtual bool BeingHeld()
	{
		return wielder != null;
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

	private void OnCollisionEnter(Collision collision)
	{
		if (rigidbody != null && isMoving)
		{
			if (FindComponent(collision.collider.transform, out Enemy enemy))
			{
				enemy.Kill();
				Destroy(gameObject);
			}
		}
	}
}
