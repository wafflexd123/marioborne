using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Humanoid
{
	//Inspector
	public static Player singlePlayer;
	public bool invincibility;
	public float maxInteractDistance;
	[SerializeField] LayerMask raycastIgnore;
	[SerializeField] public ReflectWindow reflectWindowPrefab;
	public AudioPool.Clips deathSounds;

	//Public
	public RaycastHit raycast;
	[HideInInspector] public bool hasDied;
	[HideInInspector] public PlayerMovement movement;
	[HideInInspector] public PlayerCamera cameraController;
	[HideInInspector] public Fists fists;
	[HideInInspector] public new Camera camera;
	[HideInInspector] public PowerManager powers;

	//Script
	Console.Line cnsRaycast;
	MessageManager messageManager;
	GameObject escMenu;
	Coroutine crtMoveToEnemy;
	Transform weaponHand;
	PlayerRewinder rewinder;

	[Header("IK")]
	[SerializeField] private GameObject armObject;
	[SerializeField] private Transform handTarget_L;
	[SerializeField] private Transform handTarget_R;
	private HandFollowObject handFollower_L;
	private HandFollowObject handFollower_R;
	private Animator handAnimator;

	public void IKEquip(bool leftHand, Transform newParent, string animName = "")
	{
		if (newParent == null) { Debug.LogWarning("Player has picked up a weapon with no IK target, please set one for this prefab. "); return; }
		HandFollowObject handFollower = leftHand ? handFollower_L : handFollower_R;
		handFollower.AddIKTarget(newParent);
		//print("Set hand" + (leftHand ? "_L" : "_R") + " target to: " + newParent.name);
	}

	public void IKUnequip(bool leftHand, bool resetHandAnimation = true)
	{
		HandFollowObject handFollower = leftHand ? handFollower_L : handFollower_R;
		handFollower.RemoveIKTarget();
		handAnimator.Play("empty", 2);
	}

	public override Vector3 LookDirection => camera.transform.forward;
	public override Vector3 LookingAt => raycast.point;
	public override Vector3 Velocity => movement.rigidbody.velocity;

	protected override void Awake()
	{
		base.Awake();
		if (invincibility && !Application.isEditor) invincibility = false;
		rewinder = GetComponent<PlayerRewinder>();
		powers = transform.Find("Left Hand").GetComponent<PowerManager>();
		weaponHand = transform.Find("Right Hand");
		cameraController = transform.Find("Head").GetComponent<PlayerCamera>();
		camera = cameraController.transform.Find("Eyes").Find("Camera").GetComponent<Camera>();
		fists = transform.Find("Body").Find("Hand").GetComponent<Fists>();
		singlePlayer = this;
		cnsRaycast = Console.AddLine();
		movement = GetComponent<PlayerMovement>();
		Transform ui = transform.Find("UI");
		escMenu = ui.Find("Escape Menu").gameObject;
		//wickUI = ui.Find("Wick Text").GetComponent<WickUI>();
		messageManager = ui.Find("Death Message").GetComponent<MessageManager>();
		raycastIgnore = ~raycastIgnore;//invert layermask
		reflectWindowPrefab = Instantiate(reflectWindowPrefab).Initialise(this);

		handFollower_L = handTarget_L.GetComponent<HandFollowObject>();
		handFollower_R = handTarget_R.GetComponent<HandFollowObject>();
		handAnimator = armObject.GetComponent<Animator>();
	}

	private IEnumerator Start()
	{
		if (Application.isEditor)
		{
			while (true)
			{
				if (Input.GetButtonDown("Reset"))
				{
					Time.timeScale = 1;
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
					yield break;
				}
				yield return null;
			}
		}
	}

	void Update()
	{
		if (input.enableInput)
		{
			HandleInput();
			Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out raycast, Mathf.Infinity, raycastIgnore, QueryTriggerInteraction.Ignore);
			if (FindComponent(raycast.transform, out Raycastable hit)) hit.OnRaycast(this);
			if (Input.GetButtonDown("Interact") && FindComponent(raycast.transform, out WeaponBase weaponBase)) weaponBase.Pickup(this);
			if (Console.Enabled) cnsRaycast.text = $"Looking at: {(raycast.transform != null ? raycast.transform.name : null)}";
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (escMenu.activeSelf)
			{
				escMenu.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				escMenu.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
			}
		}
	}

	/// <summary>
	/// This script must have a negative value in the script execution order list for this to work reliably
	/// </summary>
	void HandleInput()
	{
		foreach (string name in Enum.GetNames(typeof(InputAxes)))//Loop through every input axis to see if it was pressed. This can be event driven (more efficient) if we use the newer Unity Input System, but it's fine for now.
		{
			if (Input.GetButtonDown(name)) input.Press(name, () => Input.GetAxis(name), () => Input.GetAxis(name) != 0);
		}
	}

	/// <summary>Method called by a weapon when it detects the player walking over it (do not call otherwise)</summary>
	/// <returns>True if object is picked up, sets parent of weapon</returns>
	public override bool OnPickupWeapon(WeaponBase weapon)
	{
		if (!this.weapon)//if nothing in hand
		{
			weapon.transform.SetParent(weaponHand);
			IKEquip(false, weapon.IKHandTarget);
			weapon.SetRenderMode(true);
			if (weapon.animationName != "") handAnimator.Play(weapon.animationName, 2);
			return true;
		}
		return false;
	}

	public void ForceKill(DeathType deathType = DeathType.General)
	{
		KillInternal(deathType);
	}

	public override void Kill(DeathType deathType = DeathType.General)
	{
		if (invincibility) Debug.Log("You would have died, but no one can kill John Matrix.");
		else KillInternal(deathType);
	}

	void KillInternal(DeathType deathType)
	{
		if (!hasDied)
		{
			Time.timeScale = 0;
			powers.SelectPower<Rewinder>(true);
			hasDied = true;
			messageManager.DisplayRandomMessage(deathType, true);
			model.dying = true;
			movement.enabled = false;
			cameraController.enabled = false;
			input.enableInput = false;
			deathSounds.PlayRandom(model.audioPool);
			rewinder.AddFrameAction(() => ResetDeath());
		}
	}

	public void ResetDeath()
	{
		hasDied = false;
		messageManager.Hide();
		movement.Health(1, DeathType.General);//temp, have to record this with the rewind later
		model.dying = false;
		cameraController.enabled = true;
		input.enableInput = true;
	}

	public void TeleportToEnemy(Humanoid enemy, float teleportSpeed, float maxTime)
	{
		if (enemy.enabled && crtMoveToEnemy == null)//dont teleport to dead/disabled enemies; will cause issues otherwise
		{
			//Instantiate(model.deathPosePrefab, transform.position, transform.rotation);
			cameraController.enabled = false;
			movement.enabled = false;
			movement.EnableCollider(false);
			enemy.enabled = false;
			if (weapon) weapon.Drop(0);
			crtMoveToEnemy = StartCoroutine(LerpToPos(new Position(enemy.transform), Mathf.Clamp(0, maxTime, Vector3.Distance(enemy.transform.position, transform.position) / teleportSpeed), transform, () =>
			{
				if (enemy.weapon) enemy.weapon.Pickup(this, true);
				enemy.Kill();
				cameraController.enabled = true;
				movement.EnableCollider(true);
				movement.enabled = true;
				crtMoveToEnemy = null;
			}, EasingFunction.EaseInOutSine));
		}
	}

	public override void ReceiveAttack(MonoBehaviour attacker, MonoBehaviour weapon, DeathType deathType, Collision collision)
	{
		if (attacker != this) Kill(deathType);
	}
}
