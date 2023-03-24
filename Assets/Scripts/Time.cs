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
