using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Humanoid
{
	public static Player singlePlayer;
	public bool invincibility;
	public float maxInteractDistance;
	public new Camera camera;
	public GameObject deathUI;
	RaycastHit raycast;

	public override Vector3 LookDirection => camera.transform.TransformDirection(Vector3.forward);
	public override Vector3 LookingAt => raycast.point;

	protected override void Awake()
	{
		base.Awake();
		singlePlayer = this;
	}

	private void Start()
	{
		input.AddListener("Mouse", InputType.OnPress, (float direction) => PickupObject(direction));
	}

	void Update()
	{
		HandleInput();
		Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out raycast);
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

	public override void Kill()
	{
		if (invincibility) Debug.Log("You would have died, but no one can kill John Matrix.");
		else
		{
			//animatorManager.dying = true;
			deathUI.SetActive(true);
			GetComponent<AdvPlayerMovement>().enabled = false;
			Cursor.lockState = CursorLockMode.None;
			enabled = false;
		}
	}
}
