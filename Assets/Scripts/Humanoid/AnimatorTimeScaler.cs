using UnityEngine;

public class AnimatorTimeScaler : MonoBehaviour, ITimeScaleListener
{
	Animator animator;

	void Awake()
	{
		animator = GetComponent<Animator>();
		Time.timeScaleListeners.Add(this);
	}

	public void OnTimeSlow()
	{
		animator.SetFloat("timeScale", Time.timeScale);
	}

	private void OnDestroy()
	{
		Time.timeScaleListeners.Remove(this);
	}
}
