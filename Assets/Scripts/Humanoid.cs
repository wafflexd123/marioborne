using UnityEngine;
public abstract class Humanoid : MonoBehaviourPlus
{
    public Transform hand;
    protected AnimatorManager animatorManager;

    public abstract Vector3 LookDirection { get; }
    public abstract Vector3 LookingAt { get; }

    /// <returns>True if button was pressed this frame</returns>
    public abstract bool GetAxisDown(string axis, out float value);
    /// <returns>Value of axis if button is held</returns>
    public float GetAxis(string axis)
    {
        GetAxisDown(axis, out float value);
        return value;
    }
    public abstract void Kill();

    protected virtual void Awake()
    {
        animatorManager = new AnimatorManager(transform.Find("Body").GetChild(0).GetComponent<Animator>());
    }

    public class AnimatorManager
    {
        public Animator animator;
        public AnimatorManager(Animator animator)
        {
            this.animator = animator;
        }
        public Vector3 velocity
        {
            set
            {
                animator.SetFloat("xVelocity", value.x);
                animator.SetFloat("yVelocity", value.y);
                animator.SetFloat("zVelocity", value.z);
            }
        }
        public bool holdingPistol { set => animator.SetBool("holdingPistol", value); }
        public bool holdingKnife { set => animator.SetBool("holdingKnife", value); }
        public bool crouching { set => animator.SetBool("crouching", value); }
        public bool dying { set => animator.SetBool("dying", value); }
    }
}