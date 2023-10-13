using System;
using UnityEngine;

/// <summary>
/// Allows multiple sounds to play without cutting off previous sounds.
/// HOW TO USE:
/// 1) Create an AudioPool.Clip or AudioPool.Clips class, available to the inspector.
/// 2) AudioPool audioPool = AddComponent(AudioPool).Initialise(...);
/// 3) Call Clip.Play(audioPool) or Clips.PlayRandom(audioPool).
/// </summary>
public class AudioPool : MonoBehaviour, ITimeScaleListener, IRewindListener
{
	static readonly float timeSlowPitchReduction = .3f;
	AudioPlayer[] audioPlayers;
	int pos = 0, pitchDirection = 1;

	/// <param name="timeBetweenShots">Minimum time between any two sounds.</param>
	/// <param name="maxShotLength">Maximum length of any sound that will play. Clips.MaxShotLength() can be used.</param>
	/// <param name="maxAudioSources">The maximum amount of AudioSources that will be instantiated. More than 5 may have diminishing returns for short tracks.</param>
	public AudioPool Initialise(float timeBetweenShots, float maxShotLength, int maxAudioSources = 5)
	{
		if (timeBetweenShots < 0.01f) timeBetweenShots = 0.01f;
		Time.timeScaleListeners.Add(this);
		Time.rewindListeners.Add(this);
		int amount = Mathf.Clamp(Mathf.CeilToInt(maxShotLength / timeBetweenShots), 1, maxAudioSources);//create enough audiosources so clips will not cancel already playing ones, clamped to a max of 5 by default
		audioPlayers = new AudioPlayer[amount];
		GameObject g = new GameObject("Audio");
		g.transform.SetParent(transform);
		g.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		for (int i = 0; i < amount; i++)
		{
			audioPlayers[i] = new AudioPlayer(this, g.AddComponent<AudioSource>());
			audioPlayers[i].source.spatialBlend = 1;
			audioPlayers[i].source.dopplerLevel = 0;
			audioPlayers[i].source.rolloffMode = AudioRolloffMode.Linear;
			audioPlayers[i].source.minDistance = 2;
			audioPlayers[i].source.maxDistance = 40;
		}
		return this;
	}

	AudioPlayer NextAudioPlayer()
	{
		AudioPlayer source = audioPlayers[pos];
		if (++pos == audioPlayers.Length) pos = 0;
		return source;
	}

	public void OnTimeSlow()
	{
		foreach (AudioPlayer a in audioPlayers) if (a.source.isPlaying) a.TimePitch();
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

	void OnDestroy()
	{
		Time.timeScaleListeners.Remove(this);
		Time.rewindListeners.Remove(this);
	}

	class AudioPlayer
	{
		public readonly AudioSource source;
		public float startVolume, startPitch;
		readonly AudioPool pool;

		public AudioPlayer(AudioPool audioPool, AudioSource audioSource)
		{
			this.source = audioSource;
			this.pool = audioPool;
		}

		public void Play(AudioClip clip, float volume, float pitch, float maxDistance)
		{
			source.volume = startVolume = volume;
			source.pitch = startPitch = pitch;
			source.maxDistance = maxDistance;
			TimePitch();
			source.PlayOneShot(clip);
		}

		public void TimePitch()
		{
			source.pitch = Mathf.Lerp(startPitch - timeSlowPitchReduction, startPitch, Mathf.InverseLerp(Time.minTimeScale, 1, Time.timeScale)) * pool.pitchDirection;
		}
	}


	[Serializable]
	public class Clips
	{
		public Clip[] clips;

		public void PlayRandom(AudioPool audioSource, float additionalVolume = 0, float additionalPitch = 0, float additionalMaxVolume = 0)
		{
			if(clips.Length > 0) clips[UnityEngine.Random.Range(0, clips.Length)].Play(audioSource, additionalVolume, additionalPitch, additionalMaxVolume);
		}

		public float MaxShotLength()
		{
			float t = 0;
			foreach (Clip item in clips) if (item.audio.length > t) t = item.audio.length;
			return t;
		}
	}

	[Serializable]
	public class Clip
	{
		public AudioClip audio;
		public float volume = 1, pitch = 1, maxDistance = 40;

		public void Play(AudioPool audioPool, float additionalVolume = 0, float additionalPitch = 0, float additionalMaxDistance = 0)
		{
			audioPool.NextAudioPlayer().Play(audio, volume + additionalVolume, pitch + additionalPitch, maxDistance + additionalMaxDistance);
		}
	}
}
