using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimatorManager : MonoBehaviourPlus
{
	public float walkSpeed, runSpeed;
	Animator animator;

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

	public Vector3 velocity
	{
		set
		{
			float magnitude = Mathf.Sqrt((value.x * value.x) + (value.z * value.z));
			magnitude = ((int)(magnitude * 100)) / 100f;//round to 2 decimals
			animator.SetFloat("walkMagnitude", Mathf.InverseLerp(0, walkSpeed, magnitude));
			animator.SetFloat("runMagnitude", Mathf.InverseLerp(walkSpeed, runSpeed, magnitude));
		}
	}
	public bool holdingWeapon { set => animator.SetLayerWeight(1, value ? 1 : 0); }
	public bool attacking { set => animator.SetBool("attacking", value); }
	public bool crouching { set => animator.SetBool("crouching", value); }
	public bool dying { set => animator.SetBool("dying", value); }
}