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
