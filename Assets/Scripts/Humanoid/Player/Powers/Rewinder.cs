using System.Collections;
using UnityEngine;

public class Rewinder : MonoBehaviour, IPlayerPower
{
	public float rewindSpeed;
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
		if (Input.GetButtonDown(inputAxis)) StartCoroutine(TimeRoutine());
	}

	IEnumerator TimeRoutine()
	{
		Time.timeScale = 1;
		float rewind = 1;
		Time.StartRewind();
		do
		{
			Time.Rewind(Time.deltaTime * (rewind += rewindSpeed * Time.deltaTime));
			HandLeftManager.Instance.AddEnergy(handShakeLevel * rewind);
			yield return null;
		} while (Input.GetButton(inputAxis));
		Time.StopRewind();
	}
}
