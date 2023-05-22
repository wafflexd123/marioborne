using System;
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
	public static float deltaTime { get => timeScale * UnityEngine.Time.unscaledDeltaTime; }
	public static float unscaledDeltaTime { get => UnityEngine.Time.unscaledDeltaTime; }
	public static float fixedDeltaTime { get => timeScale * UnityEngine.Time.fixedUnscaledDeltaTime; }
	public static float fixedUnscaledDeltaTime { get => UnityEngine.Time.fixedUnscaledDeltaTime; }

	public static List<ITimeScaleListener> timeScaleListeners = new List<ITimeScaleListener>();

	public static void OnSceneChange()
	{
		timeScaleListeners.Clear();
	}
}

public interface ITimeScaleListener
{
	public void OnTimeSlow();
}

public class WaitForSeconds : CustomYieldInstruction
{
	float time, waitTime;
	public override bool keepWaiting { get => (time += Time.fixedDeltaTime) < waitTime; }
	public WaitForSeconds(float waitTime) { this.waitTime = waitTime; }
}

public class WaitForSecondsRealtime : CustomYieldInstruction
{
	float time, waitTime;
	public override bool keepWaiting { get => (time += Time.fixedUnscaledDeltaTime) < waitTime; }
	public WaitForSecondsRealtime(float waitTime) { this.waitTime = waitTime; }
}

