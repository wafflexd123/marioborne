using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimatorManager : MonoBehaviourPlus
{
	public float walkSpeed, runSpeed, airSpeed, landSpeed;
	bool _wallRunning, _grounded;
	Animator animator;
	Vector3 previousVelocity;
	Coroutine crtGroundState;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		if (transform.localPosition != Vector3.zero)
		{
			transform.localPosition = Vector3.zero;
			Debug.LogWarning("The model position of " + name + " is not zero. Resetting.");
		}
		if (transform.localEulerAngles != Vector3.zero)
		{
			transform.localEulerAngles = Vector3.zero;
			Debug.LogWarning("The model rotation of " + name + " is not zero. Resetting.");
		}
	}

	public void Collide(Vector3 velocity)
	{
		if (previousVelocity.y <= -landSpeed && (velocity.y < 0.01f && velocity.y > -0.01f))//if hit the ground
		{
			animator.SetTrigger("land");
		}
	}

	void CheckGroundState()
	{
		if (crtGroundState == null) crtGroundState = StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForFixedUpdate();
			animator.SetBool("falling", !_grounded && !_wallRunning);
			crtGroundState = null;
		}
	}

	public bool holdingWeapon { set => animator.SetLayerWeight(1, value ? 1 : 0); }
	public bool attacking { set => animator.SetBool("attacking", value); }
	public bool crouching { set => animator.SetBool("crouching", value); }
	public bool dying { set => animator.SetBool("dying", value); }
	public bool grounded { set { _grounded = value; CheckGroundState(); } }
	public bool wallRunning { set { _wallRunning = value; CheckGroundState(); } }
	public Vector3 velocity
	{
		set
		{
			float magnitude = Mathf.Sqrt((value.x * value.x) + (value.z * value.z));
			magnitude = ((int)(magnitude * 100)) / 100f;//round to 2 decimals
			animator.SetFloat("walkMagnitude", Mathf.InverseLerp(0, walkSpeed, magnitude));
			animator.SetFloat("runMagnitude", Mathf.InverseLerp(walkSpeed, runSpeed, magnitude));
			//animator.SetBool("falling", Mathf.Abs(value.y) >= airSpeed);
			previousVelocity = value;
		}
	}
}