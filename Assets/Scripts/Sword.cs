using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviourPlus
{
	public Position holsterPos, guardPos;
	public float guardSpeed;

	void Update()
	{
		//if pressing mouse 1, move sword to guard pos
		//when mouse 1 releases, move sword to holder

		if (Input.GetMouseButton(1))
		{
			if (transform.localPosition != guardPos.coords)
			{
				transform.localEulerAngles = guardPos.eulers;//temp
				transform.localPosition = Vector3.MoveTowards(transform.localPosition, guardPos.coords, guardSpeed * Time.deltaTime);
			}
		}
		else if (transform.localPosition != holsterPos.coords)
		{
			transform.localEulerAngles = holsterPos.eulers;//temp
			transform.localPosition = Vector3.MoveTowards(transform.localPosition, holsterPos.coords, guardSpeed * Time.deltaTime);
		}
	}
}
