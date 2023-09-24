using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandLeftManager : MonoBehaviour
{
    public static HandLeftManager Instance;
    private bool handActive = true;
    [SerializeField] private Transform IKTarget;
    [SerializeField] private Animator handAnimator;
    [SerializeField] private float energyDissipationRate = 0.32f;
    [SerializeField] private float noiseSpeed = 1f;
    [SerializeField] private float minAmplitude;
    [SerializeField] private float maxAmplitude;
    [SerializeField, Range(0.0f, 1.0f)] private float minAnimationInfluence;
    public float energy = 0;

    private Vector3 baseLocalPos;
    private Vector3 offset = Vector3.zero;
    [SerializeField] private Vector3 offscreenPos = -Vector3.up;
    [SerializeField] private float activationTime = 0.67f;

    const float c1 = 1.70158f;
    const float c3 = c1 + 1f;
    void Start()
    {
        baseLocalPos = IKTarget.localPosition;
    }

    void Update()
    {
        if (!handActive) return;
        energy -= energyDissipationRate;
        energy = Mathf.Clamp01(energy);

        handAnimator.SetLayerWeight(1, minAnimationInfluence + energy * (1f - minAnimationInfluence));

        float amp = Mathf.Lerp(minAmplitude, maxAmplitude, energy);
        offset.x = amp * NoiseHelper.GetPerlin(0f, noiseSpeed);
        offset.y = amp * NoiseHelper.GetPerlin(1f, noiseSpeed);
        offset.z = amp * NoiseHelper.GetPerlin(2f, noiseSpeed);
        IKTarget.localPosition = baseLocalPos + offset;
        print($"Energy: {energy},\t amp: {amp},\toffset: {offset}");
    }

    /// <summary>
    /// Start an animation to raise or lower the hand from the screen. 
    /// </summary>
    /// <param name="active">True => on screen, false => offscreen</param>
    public void SetHandActive(bool active)
    {
        StartCoroutine(HandActivationSequence(active));
    }

    private IEnumerator HandActivationSequence(bool newActive)
    {
        Vector3 targetPosition = newActive ? offset : offscreenPos;
        Vector3 startingPos = IKTarget.position;
        float timeRemaining = activationTime;
        while (activationTime > 0f)
        {
            float t = timeRemaining / activationTime;
            float tEased = 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2); // https://easings.net/#easeOutBack
            IKTarget.position = Vector3.LerpUnclamped(startingPos, targetPosition, tEased);
            timeRemaining -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        handActive = newActive;
    }

    public void AddEnergy(float amount) { energy += amount; }
    public void AddEnergy() { energy += energyDissipationRate; }    // sustain current amount of energy this frame. 
    public void SetEnergy(float amount) { energy = amount; }
    public void ZeroEnergy() { energy = 0f; }

    private void Awake()
    {
        Instance = this;
    }
}
