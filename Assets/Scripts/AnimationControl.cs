using UnityEngine;

public class AnimationControl: MonoBehaviour
{
    public Animator animator; 
    public float intensityMultiplier = 1.0f;

    private Rigidbody rb; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float speed = rb.velocity.magnitude; 
        animator.SetFloat("RunningSpeed", speed * intensityMultiplier);
    }
}
