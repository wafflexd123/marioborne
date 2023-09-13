using System.Collections;
using UnityEngine;

public class Rewinder : MonoBehaviour, IPlayerPower
{
	public float rewindSpeed;

	public bool CanDisable => !Input.GetButton("Ability");

	void Update()
	{
		if (Input.GetButtonDown("Ability"))
		{
			StartCoroutine(TimeRoutine());
		}
	}

	IEnumerator TimeRoutine()
	{
		float rewind = 1;
		Time.StartRewind();
		do
		{
			Time.Rewind(Time.deltaTime * (rewind += rewindSpeed * Time.deltaTime));
			yield return null;
		} while (Input.GetButton("Ability"));
		Time.StopRewind();
	}
}
