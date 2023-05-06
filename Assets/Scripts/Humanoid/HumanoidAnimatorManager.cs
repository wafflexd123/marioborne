using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimatorManager : MonoBehaviourPlus
{
	public float walkSpeed, runSpeed, fallCrouchEngageSpeed, fallRollEngageSpeed, colliderCrouchTime, crouchHeightMultiplier;

	float colliderHeight, colliderHeightCrouch, colliderCentreCrouch;
	bool _wallRunning, _grounded, queueRoll;
	Vector3 previousVelocity, colliderCentre;
	Animator animator;
	Coroutine crtGroundState, crtQueueRoll, crtCrouch;
	new CapsuleCollider collider;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		collider = transform.parent.GetComponent<CapsuleCollider>();
		colliderHeight = collider.height;
		colliderHeightCrouch = colliderHeight * crouchHeightMultiplier;
		colliderCentre = collider.center;
		colliderCentreCrouch = colliderCentre.y * crouchHeightMultiplier;

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
		else queueRoll = false;//player cannot spam roll button, must only press once
		IEnumerator Routine()
		{
			queueRoll = true;
			yield return new WaitForSeconds(queueTime);
			queueRoll = false;
			yield return new WaitForSeconds(requeueTime);
			crtQueueRoll = null;
		}
	}

	IEnumerator Crouch(bool crouch)
	{
		animator.SetBool("sliding", crouch);
		float percent, timer = 0;
		float centreStart = collider.center.y, centreEnd;
		float heightStart = collider.height, heightEnd;
		if (crouch)
		{
			centreEnd = colliderCentreCrouch;
			heightEnd = colliderHeightCrouch;
		}
		else
		{
			centreEnd = colliderCentre.y;
			heightEnd = colliderHeight;
		}
		do
		{
			timer += Time.fixedDeltaTime;
			percent = Mathf.Clamp01(timer / colliderCrouchTime);
			collider.center = new Vector3(colliderCentre.x, Mathf.Lerp(centreStart, centreEnd, percent), colliderCentre.z);
			collider.height = Mathf.Lerp(heightStart, heightEnd, percent);
			yield return new WaitForFixedUpdate();
		} while (percent != 1);
	}

	public bool holdingMelee { set => animator.SetLayerWeight(1, value ? 1 : 0); }
    public bool holdingGun { set => animator.SetLayerWeight(2, value ? 1 : 0); }
    public bool attacking { set => animator.SetBool("attacking", value); }
    public bool deflect { set => animator.SetBool("deflect", value); }
	public bool dying { set => animator.SetBool("dying", value); }
	public bool grounded { set { _grounded = value; CheckGroundState(); } }
	public bool wallRunning { set { _wallRunning = value; CheckGroundState(); } }
	public bool sliding { set => ResetRoutine(Crouch(value), ref crtCrouch); }
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