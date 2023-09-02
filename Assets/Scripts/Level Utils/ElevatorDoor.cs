using UnityEngine;
using UnityEngine.Events;

public class ElevatorDoor : MonoBehaviourPlus
{
	public EasingFunction.Enum doorEasing;
	public Transform leftDoor, rightDoor;
	public TriggerCollider triggerCollider;
	public float moveTime, zMovement;
	public bool animateOnEnable, isOpen;
	public UnityEvent playerInElevatorDoorClosedEvent, emptyElevatorDoorClosedEvent;
	PositionAndScale leftOpen, leftClosed, rightOpen, rightClosed;
	Coroutine crtLeft, crtRight;

	public bool IsFullyOpen => isOpen && crtRight == null;
	public bool IsFullyClosed => !isOpen && crtRight == null;

	private void Start()
	{
		if (isOpen)
		{
			leftOpen = new PositionAndScale(leftDoor, true);
			leftClosed = new PositionAndScale(leftDoor.localPosition - new Vector3(0, 0, zMovement), leftDoor.localEulerAngles, leftDoor.localScale);
			rightOpen = new PositionAndScale(rightDoor, true);
			rightClosed = new PositionAndScale(rightDoor.localPosition - new Vector3(0, 0, -zMovement), rightDoor.localEulerAngles, rightDoor.localScale);
		}
		else
		{
			leftClosed = new PositionAndScale(leftDoor, true);
			leftOpen = new PositionAndScale(leftDoor.localPosition + new Vector3(0, 0, zMovement), leftDoor.localEulerAngles, leftDoor.localScale);
			rightClosed = new PositionAndScale(rightDoor, true);
			rightOpen = new PositionAndScale(rightDoor.localPosition + new Vector3(0, 0, -zMovement), rightDoor.localEulerAngles, rightDoor.localScale);
		}
	}

	public void Toggle()
	{
		if (isOpen) Close();
		else Open();
	}

	public void Open()
	{
		ResetRoutine(LerpToPosLocal(leftOpen, moveTime, leftDoor, null, EasingFunction.Get(doorEasing)), ref crtLeft);
		ResetRoutine(LerpToPosLocal(rightOpen, moveTime, rightDoor, () => { isOpen = true; crtRight = null; }, EasingFunction.Get(doorEasing)), ref crtRight);
	}

	public void Close()
	{
		ResetRoutine(LerpToPosLocal(leftClosed, moveTime, leftDoor, null, EasingFunction.Get(doorEasing)), ref crtLeft);
		ResetRoutine(LerpToPosLocal(rightClosed, moveTime, rightDoor, () => { isOpen = false; crtRight = null; CheckForPlayer(); }, EasingFunction.Get(doorEasing)), ref crtRight);
	}

	void CheckForPlayer()
	{
		if (triggerCollider.isTriggered) playerInElevatorDoorClosedEvent.Invoke();
		else emptyElevatorDoorClosedEvent.Invoke();
	}

	private void OnEnable()
	{
		if (animateOnEnable) Toggle();
	}
}
