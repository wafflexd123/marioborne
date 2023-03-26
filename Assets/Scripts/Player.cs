using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Humanoid
{
	public float maxInteractDistance;
	public new Camera camera;

	void Update()
	{
		Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out raycast, maxInteractDistance);

		if (GetAxisDown("Mouse", out float value) && value < 0 && hand.childCount == 0)//if clicked mouse button && left clicked && nothing in hand
		{
			if (FindComponent(raycast.transform, out WeaponBase weapon))
			{
				weapon.Pickup(this);
			}
		}
	}

	public override bool GetAxisDown(string axis, out float value)
	{
		value = Input.GetAxis(axis);
		return Input.GetButtonDown(axis);
	}
}
