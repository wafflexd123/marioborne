using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Humanoid
{
	public float maxInteractDistance;
	public new Camera camera;
	RaycastHit raycast;

	public override Vector3 LookDirection => camera.transform.TransformDirection(Vector3.forward);
	public override Vector3 LookingAt => raycast.point;

	void Update()
	{
		Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out raycast);

		if (GetAxisDown("Mouse", out float value) && value < 0 && hand.childCount == 0)//if clicked mouse button && left clicked && nothing in hand
		{
			if (FindComponent(raycast.transform, out WeaponBase weapon))
			{
				if (Vector3.Distance(transform.position, weapon.transform.position) <= maxInteractDistance) weapon.Pickup(this);
			}
		}
	}

	public override bool GetAxisDown(string axis, out float value)
	{
		value = Input.GetAxis(axis);
		return Input.GetButtonDown(axis);
	}
}
