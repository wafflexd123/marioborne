using UnityEngine;

public class TimeslowAudio : MonoBehaviour, ITimeScaleListener
{
    AudioSource audioSource;
    float startPitch;
    public float minTimescale = .25f, minPitch = .7f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        startPitch = audioSource.pitch;
        Time.timeScaleListeners.Add(this);
    }

    public void OnTimeSlow()
    {
        audioSource.pitch = Mathf.Lerp(minPitch, startPitch, Mathf.InverseLerp(minTimescale, 1, Time.timeScale));
    }

    private void OnDestroy()
    {
        Time.timeScaleListeners.Remove(this);
    }
}
