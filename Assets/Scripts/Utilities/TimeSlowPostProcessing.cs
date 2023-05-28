using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TimeSlowPostProcessing : MonoBehaviour, ITimeScaleListener
{
    public PostProcessVolume volume;

    private ColorGrading colorGrading;
    private float initialTimeScale = 1f;
    private float targetTimeScale = 0.25f;
    private float initialContrast = 0f;
    private float targetContrast = -50f;
    private float initialHueShift = 0f;
    private float targetHueShift = 20f;
    void Start()
    {
        
        volume.profile.TryGetSettings(out colorGrading);
        Time.timeScaleListeners.Add(this);
    }
    public void OnTimeSlow()
    {
        Debug.Log("chaag");
        float t = (Time.timeScale - initialTimeScale) / (targetTimeScale - initialTimeScale);
        colorGrading.saturation.value = Mathf.Lerp(initialContrast, targetContrast, t);
        colorGrading.hueShift.value = Mathf.Lerp(initialHueShift, targetHueShift, t);
    }
}
