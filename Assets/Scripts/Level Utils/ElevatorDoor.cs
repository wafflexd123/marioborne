using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Screenshake))]
public class ElevatorDoor : UnityEventHelper
{
	public EasingFunction.Enum doorEasing = EasingFunction.Enum.EaseInOutSine;
	public Transform leftDoor, rightDoor;
	public TriggerCollider triggerCollider;
	public float moveTime, zMovement;
	public bool isOpen, enableDoors = true;
	public AudioPool.Clip audioDing, audioAmbient;
	[Tooltip("When the doors are closed, if the player in inside, the elevator will 'rise' (it plays the screenshake and ambient noise).")] public UnityEvent onEndRise;
	PositionAndScale leftOpen, leftClosed, rightOpen, rightClosed;
	Coroutine crtLeft, crtRight, crtPlayer;
	Screenshake screenshake;
	new AudioPool audio;

	public bool IsFullyOpen => isOpen && crtRight == null;
	public bool IsFullyClosed => !isOpen && crtRight == null;

	private void Start()
	{
		screenshake = GetComponent<Screenshake>();
		screenshake.objToShake = Player.singlePlayer.camera.transform;
		audio = gameObject.AddComponent<AudioPool>().Initialise(1);
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
		if (enableDoors)
		{
			ResetRoutine(LerpToPosLocal(leftOpen, moveTime, leftDoor, null, EasingFunction.Get(doorEasing)), ref crtLeft);
			ResetRoutine(LerpToPosLocal(rightOpen, moveTime, rightDoor, () => { isOpen = true; crtRight = null; }, EasingFunction.Get(doorEasing)), ref crtRight);
		}
	}

	public void Close()
	{
		if (enableDoors)
		{
			ResetRoutine(LerpToPosLocal(leftClosed, moveTime, leftDoor, null, EasingFunction.Get(doorEasing)), ref crtLeft);
			ResetRoutine(LerpToPosLocal(rightClosed, moveTime, rightDoor, () => { isOpen = false; crtRight = null; CheckForPlayer(); }, EasingFunction.Get(doorEasing)), ref crtRight);
		}
	}

	public void TeleportTo(Transform teleportTo)
	{
		Player.singlePlayer.transform.parent = transform;
		transform.position = teleportTo.position;
		Player.singlePlayer.transform.parent = null;
	}

	void CheckForPlayer()
	{
		if (triggerCollider.isTriggered)
		{
			if (crtPlayer == null) crtPlayer = StartCoroutine(PlayerInElevatorDoorClosed());
			else Debug.LogWarning("PlayerInElevatorDoorClosed called twice! This is bad.", this);
		}
	}

	IEnumerator PlayerInElevatorDoorClosed()
	{
		enableDoors = false;
		screenshake.Shake();
		audioAmbient.Play(audio);
		yield return new WaitForSeconds(screenshake.duration);
		System.Func<bool> isPlaying = audioDing.Play(audio);
		yield return new WaitWhile(isPlaying);
		enableDoors = true;
		onEndRise.Invoke();
		crtPlayer = null;
	}
}
