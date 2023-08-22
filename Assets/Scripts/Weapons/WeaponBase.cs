using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public abstract class WeaponBase : MonoBehaviourPlus
{

	[Header("Throw Variables")]
	public float throwForce = 10f; // Initial force of the throw
	public float throwSpeed = 1f; // How fast the object will go after being thrown
	public float throwFallDelay = 1f; // Delay before object starts falling

	public Position handPosition;
	public float pickupSpeed, dropForce, disablePickupAfterDropSeconds;
	public Collider[] colliders;
	protected Humanoid wielder;
	protected new Rigidbody rigidbody;
	protected RigidbodyStore rigidbodyStore;
	List<UniInput.InputAction> inputActions = new List<UniInput.InputAction>();
	Action onWielderChange;//registered by wielder when picked up, to be called just before the weapon is dropped
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

	public virtual bool Pickup(Humanoid humanoid, bool forcePickup = false)
	{
		if (forcePickup && wielder)//forced pickups only need to occur if the weapon is being wielded already
		{
			if (humanoid.PickupObject(this, out Action onWielderChange))
			{
				StopCoroutine(ref crtDropTimer);//just in case
				OnWielderChange();
				this.onWielderChange = onWielderChange;
				wielder = humanoid;
				transform.localPosition = handPosition;
				transform.localEulerAngles = handPosition.eulers;
				OnPickup();
				return true;
			}
		}
		else if (crtDropTimer == null && !wielder && humanoid.PickupObject(this, out onWielderChange))//if has been dropped for long enough, isnt being held and humanoid can pick it up
		{
			wielder = humanoid;
			EnableRigidbody(false);
			StartCoroutine(MoveToPosLocal(handPosition, pickupSpeed, transform, () => OnPickup()));//parent is set by humanoid.PickupObject()
			return true;
		}
		return false;
	}

	public virtual void Drop(float dropForce)
	{
		OnWielderChange();
		OnDrop();
		wielder = null;
		transform.parent = null;
		ResetRoutine(DropTimer(), ref crtDropTimer);
		EnableRigidbody(true);
		rigidbody.AddRelativeForce(Vector3.forward * dropForce, ForceMode.Impulse);

		IEnumerator DropTimer()
		{
			for (float i = 0; i < disablePickupAfterDropSeconds; i += Time.fixedDeltaTime) yield return null;
			crtDropTimer = null;
		}
	}

	public virtual void Throw()
	{
		OnWielderChange();
		OnDrop();
		wielder = null;
		transform.parent = null;
		ResetRoutine(ThrowTimer(), ref crtDropTimer);
		EnableRigidbody(true);
		rigidbody.AddRelativeForce(Vector3.forward * throwForce, ForceMode.Impulse);
		rigidbody.useGravity = false; // Disable gravity initially
		rigidbody.velocity = transform.forward * throwSpeed;
		StartCoroutine(ApplyGravityGradually()); // Start Coroutine to gradually apply gravity

		IEnumerator ThrowTimer()
		{
			yield return new WaitForSeconds(throwFallDelay);
			crtDropTimer = null;
		}
	}

	IEnumerator ApplyGravityGradually()
	{
		float gravityIncrement = Physics.gravity.y / throwFallDelay; // Divide gravity by delay to get increment
		float currentGravity = 0;

		while (currentGravity > Physics.gravity.y) // Continue until reaching default gravity
		{
			currentGravity += gravityIncrement * Time.deltaTime; // Increment gravity
			rigidbody.AddForce(new Vector3(0, currentGravity, 0), ForceMode.Acceleration); // Apply incremental gravity
			yield return null; // Wait for next frame
		}

		rigidbody.useGravity = true; // Enable default gravity
	}

	/// <summary>
	/// Called just after OnWielderChange() when dropped
	/// </summary>
	protected virtual void OnDrop() { }

	/// <summary>
	/// Called to prepare a weapon for being dropped OR swapping wielders without being dropped; always just before wielder is changed.
	/// </summary>
	protected virtual void OnWielderChange()
	{
		onWielderChange?.Invoke();
		onWielderChange = null;
		foreach (UniInput.InputAction action in inputActions) action.RemoveListener();
		inputActions.Clear();
	}

	/// <summary>
	/// Called when item is picked up by a humanoid. Set input listeners here.
	/// </summary>
	protected virtual void OnPickup()
	{
		inputActions.Add(wielder.input.AddListener("Attack", InputType.OnPress, (_) => Attack()));
		inputActions.Add(wielder.input.AddListener("Drop", InputType.OnPress, (_) => Drop(dropForce)));
		inputActions.Add(wielder.input.AddListener("Throw", InputType.OnPress, (_) => Throw()));
	}

	protected virtual void Attack() { }

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
