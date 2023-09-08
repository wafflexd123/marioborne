using UnityEngine;

public class TimeslowAudio : MonoBehaviour, ITimeScaleListener, IRewindListener
{
    AudioSource audioSource;
    float startPitch;
    public float minPitch = .7f;
    int pitchDirection = 1;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        startPitch = audioSource.pitch;
        Time.timeScaleListeners.Add(this);
        Time.rewindListeners.Add(this);
    }

    public void OnTimeSlow()
    {
        audioSource.pitch = Mathf.Lerp(minPitch, startPitch, Mathf.InverseLerp(Time.minTimeScale, 1, Time.timeScale)) * pitchDirection;
    }

    private void OnDestroy()
    {
        Time.timeScaleListeners.Remove(this);
        Time.rewindListeners.Remove(this);
    }

    public void Rewind(float seconds)
	{
        OnTimeSlow();
    }

	public void StartRewind()
	{
        pitchDirection = -1;
    }

    public void StopRewind()
	{
        pitchDirection = 1;
        OnTimeSlow();
    }
}
