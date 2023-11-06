using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicTransition : MonoBehaviourPlus
{
	public float fadeToVolume = 0.25f, fadeFactor;
	public bool allowMultipleFades;
	private AudioSource audioSource;
	Coroutine crtFade;

	// Start is called before the first frame update
	void Start()
	{
		audioSource = GetComponent<AudioSource>();
	}

	public void Transition(AudioClip song)
	{
		if (crtFade == null || allowMultipleFades) ResetRoutine(Fade(), ref crtFade);
		IEnumerator Fade()
		{
			while (true)//fade out by [fadeFactor] per second
			{
				audioSource.volume -= fadeFactor * Time.deltaTime;
				if (audioSource.volume <= fadeToVolume)
				{
					audioSource.volume = fadeToVolume;
					break;
				}
				else yield return null;
			}

			float time = audioSource.time;
			audioSource.clip = song;
			audioSource.time = time;
			audioSource.Play();

			while (true)//fade in by [fadeFactor] per second
			{
				audioSource.volume += fadeFactor * Time.deltaTime;
				if (audioSource.volume >= 1)
				{
					audioSource.volume = 1;
					break;
				}
				else yield return null;
			}
		}
	}

	public void FadeOut()
    {
		if (crtFade == null || allowMultipleFades) ResetRoutine(Fade(), ref crtFade);
		IEnumerator Fade()
		{
			while (true)//fade out by [fadeFactor] per second
			{
				audioSource.volume -= fadeFactor * Time.deltaTime;
				if (audioSource.volume <= 0.01f)
				{
					audioSource.volume = 0f;
					audioSource.Pause();
					break;
				}
				else yield return null;
			}
		}
	}

	public void FadeIn()
	{
		if (crtFade == null || allowMultipleFades) ResetRoutine(Fade(), ref crtFade);
		IEnumerator Fade()
		{
			while (true)//fade in by [fadeFactor] per second
			{
				audioSource.Play();
				audioSource.volume += fadeFactor * Time.deltaTime;
				if (audioSource.volume >= 1)
				{
					audioSource.volume = 1;
					break;
				}
				else yield return null;
			}
		}
	}
}
