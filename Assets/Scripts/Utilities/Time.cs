using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modifying the UnityEngine.timeScale is jittery; this is a workaround that keeps everything smooth. 
/// To access the original Time script, use UnityEngine.Time
/// </summary>
public class Time
{
	public static float timeScale
	{
		get => _timeScale;
		set
		{
			_timeScale = value;
			for (int i = 0; i < timeScaleListeners.Count; i++) timeScaleListeners[i].OnTimeSlow();
		}
	}
	static float _timeScale = 1;
	public static float deltaTime => Clamp(timeScale * UnityEngine.Time.unscaledDeltaTime, UnityEngine.Time.maximumDeltaTime);
	public static float unscaledDeltaTime => Clamp(UnityEngine.Time.unscaledDeltaTime, UnityEngine.Time.maximumDeltaTime);
	public static float fixedDeltaTime => Clamp(timeScale * UnityEngine.Time.fixedUnscaledDeltaTime, UnityEngine.Time.maximumDeltaTime);
	public static float fixedUnscaledDeltaTime => Clamp(UnityEngine.Time.fixedUnscaledDeltaTime, UnityEngine.Time.maximumDeltaTime);
	public static float targetFrameRate = 60;
	public static float maxRewindTime = 20;
	public static float minTimeScale = .33f;
	public static float currentRewind { get; private set; }
	public static bool isRewinding => currentRewind != 0;

	public static readonly List<ITimeScaleListener> timeScaleListeners = new List<ITimeScaleListener>();
	public static readonly List<IRewindListener> rewindListeners = new List<IRewindListener>();

	public static bool StartRewind()
	{
		timeScale = 1;
		foreach (IRewindListener item in rewindListeners) item.StartRewind();
		return true;//temp
	}

	public static bool Rewind(float seconds)
	{
		currentRewind = seconds;
		foreach (IRewindListener item in rewindListeners) item.Rewind(seconds);
		return true;//temp
	}

	public static void StopRewind()
	{
		currentRewind = 0;
		foreach (IRewindListener item in rewindListeners) item.StopRewind();
	}

	public static void OnSceneChange()
	{
		timeScaleListeners.Clear();
		rewindListeners.Clear();
	}

	static float Clamp(float f, float clampTo)
	{
		return f > clampTo ? clampTo : f;
	}
}

public interface IRewindListener
{
	public void Rewind(float seconds);
	public void StartRewind();
	public void StopRewind();
}

public interface ITimeScaleListener
{
	public void OnTimeSlow();
}

public class WaitForSeconds : CustomYieldInstruction
{
	private float time;
	private readonly float waitTime;

	public override bool keepWaiting { get => (time += Time.deltaTime) < waitTime; }
	public WaitForSeconds(float waitTime) { this.waitTime = waitTime; }
}

public class WaitForSecondsRealtime : CustomYieldInstruction
{
	private float time;
	private readonly float waitTime;

	public override bool keepWaiting { get => (time += Time.unscaledDeltaTime) < waitTime; }
	public WaitForSecondsRealtime(float waitTime) { this.waitTime = waitTime; }
}

