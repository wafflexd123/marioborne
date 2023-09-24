using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseHelper : MonoBehaviour
{
    public static NoiseHelper Instance;

    private void Awake()
    {
        NoiseHelper.Instance = this;
    }

    public static float GetPerlin(float offset, float speedFactor)
    {
        Vector2 samplePos = Vector2.one * ((UnityEngine.Time.unscaledTime * speedFactor) + offset);
        return Mathf.Clamp((2f * Mathf.PerlinNoise(samplePos.x, samplePos.y)) - 1f, -1f, 1f);
    }
    public static float GetPerlin(Vector2 noiseDirection, float offset, float speedFactor)
    {
        Vector2 samplePos = noiseDirection * ((UnityEngine.Time.unscaledTime * speedFactor) + offset);
        return Mathf.Clamp((2f * Mathf.PerlinNoise(samplePos.x, samplePos.y)) - 1f, -1f, 1f);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
