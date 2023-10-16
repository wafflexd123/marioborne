using System.Collections;
using UnityEngine;

public class Rewinder : MonoBehaviour, IPlayerPower
{
	public float rewindSpeed, minRewindTime;
	public string inputAxis;
	public KeyCode singleFrameDebugInputAxis;
	[SerializeField] private float handShakeLevel = 0.5f;

	public bool CanDisable => !Input.GetButton(inputAxis);

	IEnumerator Start()
	{
		if (Application.isEditor)
		{
			while (true)
			{
				if (Input.GetKeyDown(singleFrameDebugInputAxis)) StartCoroutine(TimeRoutine());
				yield return null;
			}
		}
	}

	void Update()
	{
		if (Input.GetButtonDown(inputAxis) && !Time.isRewinding) StartCoroutine(TimeRoutine());
	}

	IEnumerator TimeRoutine()
	{
		if (Time.StartRewind())
		{
			float rewind = 1, timer = 0;
			while (Time.Rewind(Time.deltaTime * (rewind += rewindSpeed * Time.deltaTime)) && (Input.GetButton(inputAxis) || timer < minRewindTime))
			{
				HandLeftManager.Instance.AddEnergy(handShakeLevel * rewind);
				timer += Time.deltaTime;
				yield return null;
			}
			Time.StopRewind();
		}
	}
}
