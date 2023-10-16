using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase, RequireComponent(typeof(AudioPool))]
public abstract class WeaponBase : MonoBehaviourPlus, ITelekinetic
{
	//Inspector
	[Header("Graphics")]
	public Transform IKHandTarget;
	[SerializeField] protected Transform modelRoot;
	public string animationName = "";
	[Header("Sound")]
	public AudioPool.Clip equipClip;
	public AudioPool.Clips fireClips;
	[Header("Throw")]
	public float throwForce = 10f; // Initial force of the throw
	public float throwSpeed = 1f; // How fast the object will go after being thrown
	public float throwFallDelay = 1f; // Delay before object starts falling
	[Header("Fire")]
	public bool automatic;
	public float fireDelay;
	[Header("Equip/Drop")]
	public bool canPickup;
	public Collider[] colliders;
	public float pickupSpeed, dropForce, disablePickupAfterDropSeconds;
	public Position playerHandPosition, enemyHandPosition;

	//Script
	protected AudioPool audioPool;
	protected Humanoid wielder;
	protected new Rigidbody rigidbody;
	protected RigidbodyStore rigidbodyStore;
	int layer;
	Telekinesis telekinesis;
	List<UniInput.InputAction> inputActions = new List<UniInput.InputAction>();
	Action onWielderChange;//registered by wielder when picked up, to be called just before the weapon is dropped
	Coroutine crtDropTimer;

	//Properties
	bool IsMoving { get => rigidbody != null && rigidbody.velocity != Vector3.zero; }
	public abstract bool IsFiring { get; }


	protected virtual void Start()
	{
		layer = gameObject.layer;
		rigidbody = GetComponent<Rigidbody>();
		audioPool = GetComponent<AudioPool>().Initialise(fireDelay, fireClips.MaxShotLength());
		if (FindComponent(transform, out Humanoid wielder)) Pickup(wielder);//set wielder if placed in hand on startup
		else EnableRigidbody(true);
	}

	public bool SwapWielder(Humanoid humanoid)
	{
		if (wielder && humanoid.PickupObject(this, out onWielderChange))
		{
			OnWielderChange();
			wielder = humanoid;
			OnPickup();
			if (wielder is Player) StartCoroutine(MoveToPosLocal(playerHandPosition, pickupSpeed, transform));
			else StartCoroutine(MoveToPosLocal(enemyHandPosition, pickupSpeed, transform));
			return true;
		}
		return false;
	}

	public virtual bool Pickup(Humanoid humanoid)
	{
		if (humanoid is not Player || canPickup)//why
		{
			if (crtDropTimer == null && !wielder && humanoid.PickupObject(this, out onWielderChange))//if has been dropped for long enough, isnt being held and humanoid can pick it up
			{
				wielder = humanoid;
				if (telekinesis != null) telekinesis.ReleaseObject();
				EnableRigidbody(false);
				if (wielder is Player) StartCoroutine(MoveToPosLocal(playerHandPosition, pickupSpeed, transform, () => OnPickup()));//parent is set by humanoid.PickupObject()
				else StartCoroutine(MoveToPosLocal(enemyHandPosition, pickupSpeed, transform, () => OnPickup()));
				return true;
			}
		}
		return false;
	}

	public virtual void Drop(float dropForce, bool useDropTimer = true)
	{
		OnWielderChange();
		OnDrop();
		if (wielder is Player)
		{
			Player.singlePlayer.IKUnequip(false);
			SetRenderMode(false);
		}
		wielder = null;
		transform.parent = null;
		if (useDropTimer) ResetRoutine(DropTimer(), ref crtDropTimer);
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
		equipClip.Play(audioPool);
		Sound.MakeSound(transform.position, equipClip.maxDistance, wielder);
		inputActions.Add(wielder.input.AddListener("Attack", automatic ? InputType.OnHold : InputType.OnPress, (_) => Attack()));
		inputActions.Add(wielder.input.AddListener("Drop", InputType.OnPress, (_) => Drop(dropForce)));
		inputActions.Add(wielder.input.AddListener("Throw", InputType.OnPress, (_) => Throw()));
	}

	/// <summary>
	/// Changes the layer and shadow casting mode on an object when picked up or dropped by the player. 
	/// </summary>
	public void SetRenderMode(bool playerHeld)
	{
		if (modelRoot == null) { Debug.LogWarning(name + " has not had its model root set, cannot update render mode, please set in inspector."); return; }
		for (int i = 0; i < modelRoot.childCount; i++)
		{
			SetRenderModeRecursive(modelRoot.GetChild(i), playerHeld);
		}
	}

	protected virtual void SetRenderModeRecursive(Transform obj, bool playerHeld)
	{
		obj.gameObject.layer = playerHeld ? 16 : 0; // set to first person draw mode, or default
		try
		{
			MeshRenderer mr = obj.GetComponent<MeshRenderer>();
			mr.shadowCastingMode = playerHeld ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;
		}
		catch { }

		for (int child = 0; child < obj.childCount; child++)
			SetRenderModeRecursive(obj.GetChild(child), playerHeld);
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
			gameObject.AddComponent<BasicRewindable>();
		}
		else if (rigidbody != null)
		{
			rigidbodyStore = new RigidbodyStore(rigidbody);//store rigidbody data
			Destroy(rigidbody);
			Destroy(gameObject.GetComponent<BasicRewindable>());
			for (int i = 0; i < colliders.Length; i++) colliders[i].isTrigger = true;
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (FindComponent(other.transform, out Player player)) Pickup(player);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (FindComponent(other.transform, out Player player)) Pickup(player);
	}

	public void TelekineticGrab(Telekinesis t)
	{
		if (rigidbody != null) rigidbody.useGravity = false;
		gameObject.layer = 17;
		telekinesis = t;
		if (wielder) Drop(0, false);
	}

	public void TelekineticRelease()
	{
		if (rigidbody != null) rigidbody.useGravity = true;
		gameObject.layer = layer;
		telekinesis = null;
	}
}
