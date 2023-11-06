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
	public bool enableDoors = true;
	public AudioPool.Clip audioDing, audioAmbient, audioMusic;
	[Tooltip("When the doors are closed, if the player in inside, the elevator will 'rise' (it plays the screenshake and ambient noise). Use this to call TeleportTo() or LoadScene().")] public UnityEvent onEndRise;
	[Tooltip("Called when the doors are closed and the elevator is empty (use this to re-open the doors if there is no exterior button)")] public UnityEvent onEmpty;
	[Tooltip("Called when the doors are closed and the elevator has a player in it (use this to play elevator music or whatever)")] public UnityEvent onFull;
	PositionAndScale leftOpen, leftClosed, rightOpen, rightClosed;
	Coroutine crtLeft, crtRight, crtPlayer;
	Screenshake screenshake;
	new AudioPool audio;
	bool isOpen;

	public bool IsFullyOpen => isOpen && crtRight == null;
	public bool IsFullyClosed => !isOpen && crtRight == null;

	private void Start()
	{
		screenshake = GetComponent<Screenshake>();
		screenshake.objToShake = Player.singlePlayer.camera.transform;
		audio = gameObject.AddComponent<AudioPool>().Initialise(2);
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
			isOpen = true;
			ResetRoutine(LerpToPosLocal(leftOpen, moveTime, leftDoor, () => crtLeft = null, EasingFunction.Get(doorEasing)), ref crtLeft);
			ResetRoutine(LerpToPosLocal(rightOpen, moveTime, rightDoor, () => crtRight = null, EasingFunction.Get(doorEasing)), ref crtRight);
		}
	}

	public void Close()
	{
		if (enableDoors)
		{
			isOpen = false;
			ResetRoutine(LerpToPosLocal(leftClosed, moveTime, leftDoor, () => crtLeft = null, EasingFunction.Get(doorEasing)), ref crtLeft);
			ResetRoutine(LerpToPosLocal(rightClosed, moveTime, rightDoor, () => { crtRight = null; CheckForPlayer(); }, EasingFunction.Get(doorEasing)), ref crtRight);
		}
	}

	public void TeleportTo(Transform teleportTo)
	{
		transform.SetPositionAndRotation(teleportTo.position, teleportTo.rotation);
		Player.singlePlayer.cameraController.enabled = false;
		Player.singlePlayer.transform.SetPositionAndRotation(teleportTo.position, teleportTo.rotation);
		Player.singlePlayer.cameraController.enabled = true;
	}

	void CheckForPlayer()
	{
		if (triggerCollider.isTriggered)
		{
			if (crtPlayer == null) crtPlayer = StartCoroutine(PlayerInElevatorDoorClosed());
			else Debug.LogWarning("PlayerInElevatorDoorClosed called twice! This is bad.", this);
		}
		else
		{
			onEmpty.Invoke();
		}
	}

	IEnumerator PlayerInElevatorDoorClosed()
	{
		onFull.Invoke();
		enableDoors = false;
		screenshake.Shake();
		audioAmbient.Play(audio);
		audioMusic.Play(audio);
		yield return new WaitForSeconds(screenshake.duration);
		System.Func<bool> isPlaying = audioDing.Play(audio);
		yield return new WaitWhile(isPlaying);
		enableDoors = true;
		Open();
		enableDoors = false;
		onEndRise.Invoke();
		crtPlayer = null;
	}
}
