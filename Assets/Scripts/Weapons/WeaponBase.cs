using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public abstract class WeaponBase : MonoBehaviourPlus
{
	public Position handPosition;
	public float pickupSpeed, dropForce, disablePickupAfterDropSeconds;
	public Collider[] colliders;
	protected Humanoid wielder;
	protected new Rigidbody rigidbody;
	protected RigidbodyStore rigidbodyStore;
	List<UniInput.InputAction> inputActions = new List<UniInput.InputAction>();
	Action onDrop;
	Coroutine crtDropTimer;

	bool IsMoving { get => rigidbody != null && rigidbody.velocity != Vector3.zero; }

	public abstract bool IsFiring { get; }

	protected virtual void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		if (FindComponent(transform, out Humanoid wielder))
		{
			Pickup(wielder);//set wielder if placed in hand on startup
		}
	}

	public virtual bool Pickup(Humanoid humanoid)
	{
		if (crtDropTimer == null && !wielder && humanoid.PickupObject(this, out onDrop))//if has been dropped for long enough, isnt being held and humanoid can pick it up
		{
			wielder = humanoid;
			EnableRigidbody(false);
			StartCoroutine(LerpToPos(transform, handPosition, pickupSpeed, () => OnPickup()));
			return true;
		}
		return false;
	}

	public virtual void Drop()
	{
		ResetRoutine(DropTimer(), ref crtDropTimer);
		OnDrop();
		onDrop?.Invoke();
		onDrop = null;
		wielder = null;
		transform.parent = null;
		EnableRigidbody(true);
		rigidbody.AddRelativeForce(Vector3.forward * dropForce, ForceMode.Impulse);

		IEnumerator DropTimer()
		{
			for (float i = 0; i < disablePickupAfterDropSeconds; i += Time.fixedDeltaTime) yield return null;
			crtDropTimer = null;
		}
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

	private void OnCollisionEnter(Collision other)
	{
		if (FindComponent(other.transform, out Player player)) Pickup(player);
	}

	//private void OnCollisionEnter(Collision collision)
	//{
	//	if (rigidbody != null && IsMoving)//needs to kill at fast enough speeds, otherwise it kills even if it's barely moving. Also need animations for knockback..
	//	{
	//		if (FindComponent(collision.collider.transform, out Enemy enemy))
	//		{
	//			enemy.Kill();
	//			Destroy(gameObject);
	//		}
	//	}
	//}
}
