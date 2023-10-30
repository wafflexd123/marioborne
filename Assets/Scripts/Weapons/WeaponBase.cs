using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase, RequireComponent(typeof(AudioPool))]
public abstract class WeaponBase : MonoBehaviourPlus, ITelekinetic
{
	//Inspector
	[Header("Fire")]
	public bool automatic;
	public float fireDelay;
	[Header("Equip/Drop")]
	public Collider[] colliders;
	[Tooltip("Time until gravity reaches maxium after a throw")] public float throwFallDelay = 1f;
	public float throwForce, pickupSpeed, disablePickupAfterDropSeconds;
	public Position playerHandPosition, enemyHandPosition;
	[Header("Graphics")]
	public Transform IKHandTarget;
	[SerializeField] protected Transform modelRoot;
	public string animationName = "";
	[Header("Sound")]
	public AudioPool.Clip equipClip;
	public AudioPool.Clips fireClips;

	//Script
	protected AudioPool audioPool;
	protected new Rigidbody rigidbody;
	protected RigidbodyStore rigidbodyStore;
	int layer;
	bool canPickUp = true;
	Telekinesis telekinesis;
	Humanoid _wielder, lastWielder;
	List<UniInput.InputAction> inputActions = new List<UniInput.InputAction>();
	Action onWielderChange;//registered by wielder when picked up, to be called just before the weapon is dropped
	Coroutine crtThrow;

	//Properties
	public abstract bool IsFiring { get; }
	public Humanoid wielder { get => _wielder; protected set { lastWielder = _wielder; OnWielderChange(); _wielder = value; } }


	protected virtual void Start()
	{
		layer = gameObject.layer;
		rigidbody = GetComponent<Rigidbody>();
		audioPool = GetComponent<AudioPool>().Initialise(fireDelay, fireClips.MaxShotLength());
		if (FindComponent(transform, out Humanoid wielder)) Pickup(wielder);//set wielder if placed in hand on startup
		else EnableRigidbody(true);
	}

	public virtual bool Pickup(Humanoid humanoid, bool forceWielderChange = false)
	{
		if (canPickUp && (!wielder || forceWielderChange) && humanoid.OnPickupWeapon(this))//if has been dropped for long enough, isnt being held and humanoid can pick it up
		{
			StopCoroutine(ref crtThrow);
			wielder = humanoid;
			wielder.weapon = this;
			if (telekinesis != null) telekinesis.ReleaseObject();
			EnableRigidbody(false);
			StartCoroutine(MoveToPosLocal(wielder is Player ? playerHandPosition : enemyHandPosition, pickupSpeed, transform, () => OnPickup()));//parent is set by humanoid.OnPickupWeapon()
			return true;
		}
		return false;
	}

	public virtual void Drop(float throwForceMultiplier = 1, bool useDropTimer = true)
	{
		crtThrow = StartCoroutine(E()); // Start Coroutine to gradually apply gravity
		IEnumerator E()
		{
			if (wielder is Player)
			{
				Player.singlePlayer.IKUnequip(false);
				SetRenderMode(false);
			}
			wielder.weapon = null;
			wielder = null;
			transform.parent = null;
			EnableRigidbody(true);
			rigidbody.useGravity = false; // Disable gravity initially
			rigidbody.velocity = throwForceMultiplier * throwForce * transform.forward;
			canPickUp = !useDropTimer;

			float timer = 0;
			float gravityIncrement = Physics.gravity.y / throwFallDelay; // Divide gravity by delay to get increment
			float currentGravity = 0;

			while (currentGravity > Physics.gravity.y) // Continue until reaching default gravity
			{
				timer += Time.deltaTime;
				if (timer >= disablePickupAfterDropSeconds) canPickUp = true;
				currentGravity += gravityIncrement * Time.deltaTime; // Increment gravity
				rigidbody.AddForce(new Vector3(0, currentGravity, 0), ForceMode.Acceleration); // Apply incremental gravity
				yield return null; // Wait for next frame
			}

			while (timer < disablePickupAfterDropSeconds)
			{
				timer += Time.deltaTime;
			}
			canPickUp = true;

			rigidbody.useGravity = true; // Enable default gravity
			crtThrow = null;
		}
	}

	/// <summary>
	/// Called just before wielder is changed.
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
		inputActions.Add(wielder.input.AddListener("Drop", InputType.OnPress, (_) => Drop(1, wielder is Player)));
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
		if (FindComponent(other.transform, out Humanoid human))
		{
			if (human is Player)
			{
				if (canPickUp) Pickup(human);
			}
			else if (crtThrow != null)
			{
				human.ReceiveAttack(lastWielder, this, DeathType.General, other);
				Destroy(gameObject);
			}
		}
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
