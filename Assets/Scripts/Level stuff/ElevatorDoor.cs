using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorDoor : MonoBehaviourPlus
{
	public Transform leftDoor, rightDoor;
	public float speed, zMovement;
	public bool animateOnFirstFrame, isOpen;
	Position leftOpen, leftClosed, rightOpen, rightClosed;
	Coroutine crtLeft, crtRight;

	public bool IsFullyOpen => isOpen && crtRight == null;
	public bool IsFullyClosed => !isOpen && crtRight == null;

	private void Start()
	{
		if (isOpen)
		{
			leftOpen = new Position(leftDoor, true);
			leftClosed = new Position(leftDoor.localPosition - new Vector3(0, 0, zMovement), leftDoor.localEulerAngles);
			rightOpen = new Position(rightDoor, true);
			rightClosed = new Position(rightDoor.localPosition - new Vector3(0, 0, -zMovement), rightDoor.localEulerAngles);
		}
		else
		{
			leftClosed = new Position(leftDoor, true);
			leftOpen = new Position(leftDoor.localPosition + new Vector3(0, 0, zMovement), leftDoor.localEulerAngles);
			rightClosed = new Position(rightDoor, true);
			rightOpen = new Position(rightDoor.localPosition + new Vector3(0, 0, -zMovement), rightDoor.localEulerAngles);
		}

		if (animateOnFirstFrame) Toggle();
	}

	public void Toggle()
	{
		if (isOpen) Close();
		else Open();
	}

	public void Open()
	{
		if (!isOpen)
		{
			ResetRoutine(LerpToPos(leftDoor, leftOpen, speed), ref crtLeft);
			ResetRoutine(LerpToPos(rightDoor, rightOpen, speed, () => { isOpen = true; crtRight = null; }), ref crtRight);
		}
	}

	public void Close()
	{
		if (isOpen)
		{
			ResetRoutine(LerpToPos(leftDoor, leftClosed, speed), ref crtLeft);
			ResetRoutine(LerpToPos(rightDoor, rightClosed, speed, () => { isOpen = false; crtRight = null; }), ref crtRight);
		}
	}
}
