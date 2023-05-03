using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Humanoid
{
	public static Player singlePlayer;
	public bool invincibility;
	public float maxInteractDistance;
	public GameObject deathUI;
	public WickUI wickUI;
	[HideInInspector] public new Camera camera;
	public RaycastHit raycast;
	Console.Line cnsRaycast;

	public override Vector3 LookDirection => camera.transform.forward;
	public override Vector3 LookingAt => raycast.point;

	protected override void Awake()
	{
		base.Awake();
		camera = transform.Find("Head").Find("Eyes").Find("Camera").GetComponent<Camera>();
		singlePlayer = this;
		cnsRaycast = Console.AddLine();
	}

	private void Start()
	{
		input.AddListener("Mouse", InputType.OnPress, (float direction) => PickupObject(direction));
	}

	void Update()
	{
		HandleInput();
		Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out raycast);
		if (Console.Enabled) cnsRaycast.text = $"Looking at: {(raycast.collider.transform != null ? raycast.collider.transform.name : null)}";
		if (FindComponent(raycast.collider.transform, out Raycastable hit)) hit.OnRaycast(this);
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

	/// <summary>
	/// Called when mouse is pressed
	/// </summary>
	void PickupObject(float direction)
	{
		if (direction < 0 && hand.childCount == 0)//if clicked mouse button && left clicked && nothing in hand
		{
			if (FindComponent(raycast.transform, out WeaponBase weapon))
			{
				if (Vector3.Distance(transform.position, weapon.transform.position) <= maxInteractDistance) weapon.Pickup(this);
			}
		}
	}

	public override void Kill(DeathType deathType = DeathType.General)
	{
		if (invincibility) Debug.Log("You would have died, but no one can kill John Matrix.");
		else if (enabled)//if havent already died
		{
			wickUI.DisplayRandom(deathType);
			//animatorManager.dying = true;
			deathUI.SetActive(true);
			GetComponent<PlayerMovement>().enabled = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			enabled = false;
		}
	}
}
