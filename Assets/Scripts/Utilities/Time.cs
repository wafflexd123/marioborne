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
    public static float deltaTime { get => Clamp(timeScale * UnityEngine.Time.unscaledDeltaTime, UnityEngine.Time.maximumDeltaTime); }
    public static float unscaledDeltaTime { get => Clamp(UnityEngine.Time.unscaledDeltaTime, UnityEngine.Time.maximumDeltaTime); }
    public static float fixedDeltaTime { get => Clamp(timeScale * UnityEngine.Time.fixedUnscaledDeltaTime, UnityEngine.Time.maximumDeltaTime); }
    public static float fixedUnscaledDeltaTime { get => Clamp(UnityEngine.Time.fixedUnscaledDeltaTime, UnityEngine.Time.maximumDeltaTime); }

    public static readonly List<ITimeScaleListener> timeScaleListeners = new List<ITimeScaleListener>();

    public static void OnSceneChange()
    {
        timeScaleListeners.Clear();
    }

    static float Clamp(float f, float clampTo)
    {
        return f > clampTo ? clampTo : f;
    }
}

public interface ITimeScaleListener
{
    public void OnTimeSlow();
}

public class WaitForSeconds : CustomYieldInstruction
{
    float time, waitTime;
    public override bool keepWaiting { get => (time += Time.deltaTime) < waitTime; }
    public WaitForSeconds(float waitTime) { this.waitTime = waitTime; }
}

public class WaitForSecondsRealtime : CustomYieldInstruction
{
    float time, waitTime;
    public override bool keepWaiting { get => (time += Time.unscaledDeltaTime) < waitTime; }
    public WaitForSecondsRealtime(float waitTime) { this.waitTime = waitTime; }
}

