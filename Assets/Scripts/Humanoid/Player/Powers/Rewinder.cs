using System.Collections;
using UnityEngine;

public class Rewinder : MonoBehaviour, IPlayerPower
{
	public float rewindSpeed;
	[SerializeField] private float handShakeLevel = 0.5f;

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
		Time.timeScale = 1;
		float rewind = 1;
		Time.StartRewind();
		HandLeftManager.Instance.AddEnergy(handShakeLevel);
		do
		{
			Time.Rewind(Time.deltaTime * (rewind += rewindSpeed * Time.deltaTime));
			HandLeftManager.Instance.AddEnergy();
            yield return null;
		} while (Input.GetButton("Ability"));
		Time.StopRewind();
	}

    //public void HandleHand() { }
}
