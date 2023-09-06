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
		Time.StartRewind();
		do
		{
			Time.Rewind(Time.deltaTime * rewindSpeed);
			yield return null;
		} while (Input.GetButton("Ability"));
		Time.StopRewind();
	}
}
