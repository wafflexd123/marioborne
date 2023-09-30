using System;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
	AudioSource[] audioSources;
	int pos = 0;

	public AudioPool Initialise(float timeBetweenShots, float maxShotLength, Transform transform, int maxAudioSources = 5)
	{
		int amount = Mathf.Clamp(Mathf.CeilToInt(maxShotLength / timeBetweenShots), 0, maxAudioSources);//create enough audiosources so clips will not cancel already playing ones, clamped to a max of 5 by default
		audioSources = new AudioSource[amount];
		GameObject g = new GameObject("Audio");
		g.transform.SetParent(transform);
		for (int i = 0; i < amount; i++)
		{
			audioSources[i] = g.AddComponent<AudioSource>();
			audioSources[i].dopplerLevel = 0;
		}
		return this;
	}

	public AudioSource NextAudioSource()
	{
		AudioSource source = audioSources[pos];
		if (++pos == audioSources.Length) pos = 0;
		return source;
	}

	[Serializable]
	public class Clips
	{
		public Clip[] clips;

		public void PlayRandom(AudioPool audioSource, float additionalVolume = 0, float additionalPitch = 0)
		{
			clips[UnityEngine.Random.Range(0, clips.Length)].Play(audioSource, additionalVolume, additionalPitch);
		}

		public float MaxShotLength()
		{
			float t = 0;
			foreach (Clip item in clips)
			{
				if (item.audio.length > t) t = item.audio.length;
			}
			return t;
		}
	}

	[Serializable]
	public class Clip
	{
		public AudioClip audio;
		public float volume, pitch;

		public void Play(AudioPool audioSource, float additionalVolume = 0, float additionalPitch = 0)
		{
			AudioSource source = audioSource.NextAudioSource();
			source.volume = volume + additionalVolume;
			source.pitch = pitch + additionalPitch;
			source.PlayOneShot(audio);
		}
	}
}
