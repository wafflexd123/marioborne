using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Allows multiple sounds to play without cutting off previous sounds.
/// HOW TO USE:
/// 1) Create an AudioPool.Clip or AudioPool.Clips class, available to the inspector.
/// 2) AudioPool audioPool = AddComponent(AudioPool).Initialise(...);
/// 3) Call Clip.Play(audioPool) or Clips.PlayRandom(audioPool).
/// </summary>
public class AudioPool : MonoBehaviourPlus, ITimeScaleListener, IRewindListener
{
	static readonly float timeSlowPitchReduction = .3f;
	AudioPlayer[] audioPlayers;
	int pos = 0, pitchDirection = 1;

	/// <param name="timeBetweenShots">Minimum time between any two sounds.</param>
	/// <param name="maxShotLength">Maximum length of any sound that will play. Clips.MaxShotLength() can be used.</param>
	/// <param name="maxAudioSources">The maximum amount of AudioSources that will be instantiated. More than 5 may have diminishing returns for short tracks.</param>
	public AudioPool Initialise(float timeBetweenShots, float maxShotLength, int maxAudioSources = 5)
	{
		if (timeBetweenShots < 0.01f) timeBetweenShots = 0.01f;//clamp to above 0
		return Initialise(Mathf.Clamp(Mathf.CeilToInt(maxShotLength / timeBetweenShots), 1, maxAudioSources));//create enough audiosources so clips will not cancel already playing ones, clamped to a max of 5 by default
	}

	public AudioPool Initialise(int audioSourceCount)
	{
		Time.timeScaleListeners.Add(this);
		Time.rewindListeners.Add(this);
		audioPlayers = new AudioPlayer[audioSourceCount];
		GameObject g = new GameObject("Audio");
		g.transform.SetParent(transform);
		g.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		for (int i = 0; i < audioSourceCount; i++) audioPlayers[i] = new AudioPlayer(this, AddAudioSource(g));
		return this;
	}

	/// <summary>
	/// Adds an audiosource to Gameobject g, setting its values to a better default
	/// </summary>
	public static AudioSource AddAudioSource(GameObject g)
	{
		AudioSource a = g.AddComponent<AudioSource>();
		a.spatialBlend = 1;
		a.dopplerLevel = 0;
		a.rolloffMode = AudioRolloffMode.Linear;
		a.minDistance = 2;
		a.maxDistance = 40;
		return a;
	}

	AudioPlayer NextAudioPlayer()
	{
		AudioPlayer source = audioPlayers[pos];
		if (++pos == audioPlayers.Length) pos = 0;
		return source;
	}

	public void OnTimeSlow()
	{
		foreach (AudioPlayer a in audioPlayers)
		{
			if (a.source == null)//if another script destroys the audio players
			{
				Destroy(this);
				return;
			}
			if (a.source.isPlaying) a.TimePitch();
		}
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
		Coroutine crtVolume;

		public AudioPlayer(AudioPool audioPool, AudioSource audioSource)
		{
			this.source = audioSource;
			this.pool = audioPool;
		}

		/// <returns>True while audio is playing (any audio on the audioSource, not necessarily this clip)</returns>
		public Func<bool> Play(AudioClip clip, float volume, float pitch, float maxDistance, bool loop, AnimationCurve volumeCurve, float volumeCurveTime)
		{
			source.volume = startVolume = volume;
			source.pitch = startPitch = pitch;
			source.maxDistance = maxDistance;
			source.loop = loop;
			source.clip = clip;
			TimePitch();
			if (volumeCurve.length > 0 && volumeCurveTime > 0) pool.ResetRoutine(VolumeCurve(volumeCurve, volumeCurveTime), ref crtVolume);
			else pool.StopCoroutine(ref crtVolume);
			source.Play();
			return () => source.isPlaying;
		}

		IEnumerator VolumeCurve(AnimationCurve volumeCurve, float volumeCurveTime)
		{
			for (float i = 0; i < source.clip.length; i += Time.deltaTime)
			{
				source.volume = Mathf.Lerp(0, startVolume, volumeCurve.Evaluate(i / volumeCurveTime));
				yield return null;
			}
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

		/// <returns>True while audio is playing (any audio on the audioSource, not necessarily this clip)</returns>
		public Func<bool> PlayRandom(AudioPool audioSource, float additionalVolume = 0, float additionalPitch = 0, float additionalMaxVolume = 0)
		{
			return clips.Length > 0 ? clips[UnityEngine.Random.Range(0, clips.Length)].Play(audioSource, additionalVolume, additionalPitch, additionalMaxVolume) : () => false;
		}

		/// <returns>Duration of longest clip in array</returns>
		public float MaxShotLength()
		{
			float t = 0;
			foreach (Clip item in clips) if (item.audio != null && item.audio.length > t) t = item.audio.length;
			return t;
		}
	}

	[Serializable]
	public class Clip
	{
		public AudioClip audio;
		public float volume = 1, pitch = 1, maxDistance = 40;
		public bool loop = false;
		public AnimationCurve volumeCurve;
		[Tooltip("Set to 0 to ignore volume curve")] public float volumeCurveTime = 0;

		/// <returns>True while audio is playing (any audio on the audioSource, not necessarily this clip)</returns>
		public Func<bool> Play(AudioPool audioPool, float additionalVolume = 0, float additionalPitch = 0, float additionalMaxDistance = 0)
		{
			return audioPool.NextAudioPlayer().Play(audio, volume + additionalVolume, pitch + additionalPitch, maxDistance + additionalMaxDistance, loop, volumeCurve, volumeCurveTime);
		}
	}
}
