using UnityEngine;

public class AnimatorTimeScaler : MonoBehaviour, ITimeScaleListener
{
    public string timeScaleParam = "timeScale"; 
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        Time.timeScaleListeners.Add(this);
    }

    public void OnTimeSlow()
    {
        animator.SetFloat(timeScaleParam, Time.timeScale);
    }

    private void OnDestroy()
    {
        Time.timeScaleListeners.Remove(this);
    }
}
