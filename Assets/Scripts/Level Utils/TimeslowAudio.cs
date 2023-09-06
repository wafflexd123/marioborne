using UnityEngine;

public class TimeslowAudio : MonoBehaviour, ITimeScaleListener
{
    AudioSource audioSource;
    float startPitch;
    public float minPitch = .7f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        startPitch = audioSource.pitch;
        Time.timeScaleListeners.Add(this);
    }

    public void OnTimeSlow()
    {
        audioSource.pitch = Mathf.Lerp(minPitch, startPitch, Mathf.InverseLerp(Time.minTimeScale, 1, Time.timeScale));
    }

    private void OnDestroy()
    {
        Time.timeScaleListeners.Remove(this);
    }
}
