using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimatorManager : MonoBehaviourPlus
{
	public float walkSpeed, runSpeed, airSpeed, fallCrouchEngageSpeed, fallRollEngageSpeed;
	bool _wallRunning, _grounded, queueRoll;
	Animator animator;
	Vector3 previousVelocity;
	Coroutine crtGroundState, crtQueueRoll;

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
		if (queueRoll)
		{
			if (previousVelocity.y <= -fallRollEngageSpeed && Mathf.Abs(velocity.y) < 0.01f)//if hit the ground at roll speed
			{
				animator.SetTrigger("roll");
				queueRoll = false;
				StopCoroutine(crtQueueRoll);
				crtQueueRoll = null;
			}
		}
		else //if not queueing a roll or rolls are disabled
		{
			if (previousVelocity.y <= -fallCrouchEngageSpeed && Mathf.Abs(velocity.y) < 0.01f) //if hit the ground at crouch speed
			{
				animator.SetTrigger("land");
				if (crtQueueRoll != null)//if a queued roll is currently waiting for requeueTime, re-enable rolls
				{
					StopCoroutine(crtQueueRoll);
					crtQueueRoll = null;
				}
			}
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

	/// <summary>
	/// Will let a roll initiate when you hit the ground if the button was pressed during queueTime, will not let a roll initiate afterwards during requeueTime or until the frame after you hit the ground
	/// </summary>
	public void QueueRoll(float queueTime, float requeueTime)
	{
		if (crtQueueRoll == null) crtQueueRoll = StartCoroutine(Routine());
		IEnumerator Routine()
		{
			queueRoll = true;
			yield return new WaitForSeconds(queueTime);
			queueRoll = false;
			yield return new WaitForSeconds(requeueTime);
			crtQueueRoll = null;
		}
	}

	public bool holdingWeapon { set => animator.SetLayerWeight(1, value ? 1 : 0); }
	public bool attacking { set => animator.SetBool("attacking", value); }
	public bool crouching { set => animator.SetBool("crouching", value); }
	public bool dying { set => animator.SetBool("dying", value); }
	public bool grounded { set { _grounded = value; CheckGroundState(); } }
	public bool wallRunning { set { _wallRunning = value; CheckGroundState(); } }
	public bool sliding { set => animator.SetBool("sliding", value); }
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