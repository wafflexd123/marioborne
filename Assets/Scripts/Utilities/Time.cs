using UnityEngine;
using System;

/// <summary>
/// Modifying the UnityEngine.timeScale is jittery; this is a workaround that keeps everything smooth. 
/// To access the original Time script, use UnityEngine.Time
/// </summary>
public class Time
{
    public static float timeScale = 1;
    public static float deltaTime { get => timeScale * UnityEngine.Time.unscaledDeltaTime; }
    public static float unscaledDeltaTime { get => UnityEngine.Time.unscaledDeltaTime; }
    public static float fixedDeltaTime { get => timeScale * UnityEngine.Time.fixedUnscaledDeltaTime; }
    public static float fixedUnscaledDeltaTime { get => UnityEngine.Time.fixedUnscaledDeltaTime; }
 
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
