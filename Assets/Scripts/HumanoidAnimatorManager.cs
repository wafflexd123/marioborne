using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimatorManager : MonoBehaviourPlus
{
	public float walkSpeed;
	Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	public Vector3 velocity
	{
		set
		{
			animator.SetFloat("xMagnitude", Mathf.Abs(value.x));
			animator.SetInteger("xDirection", (int)Mathf.Sign(value.x));
			animator.SetFloat("yMagnitude", Mathf.Abs(value.y));
			animator.SetInteger("yDirection", (int)Mathf.Sign(value.y));
			animator.SetFloat("zMagnitude", Mathf.Abs(value.z));
			animator.SetInteger("zDirection", (int)Mathf.Sign(value.z));
			animator.SetBool("walking", value.x != 0 || value.z != 0);
			animator.SetLayerWeight(1, Mathf.InverseLerp(0, walkSpeed, Mathf.Abs(value.x)));
		}
	}
	public bool holdingWeapon { set => animator.SetLayerWeight(2, value ? 1 : 0); }
	public bool triggerWeapon { set => animator.SetBool("triggerWeapon", value); }
	public bool crouching { set => animator.SetBool("crouching", value); }
	public bool dying { set => animator.SetBool("dying", value); }
}