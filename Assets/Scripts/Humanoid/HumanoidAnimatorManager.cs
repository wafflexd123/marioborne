using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HumanoidAnimatorManager : MonoBehaviourPlus
{
	//Inspector
	public AudioClip[] footstepSounds, jumpingSounds, landingSounds;
	public float footStepSoundRadius, walkSpeed, runSpeed, colliderCrouchTime, crouchHeightMultiplier, minStepVolume, maxStepVolume, velocityAtMaxStepVolume;
	public GameObject deathPosePrefab;

	//Script
	private AudioSource audioSource;
	private float colliderHeight, colliderHeightCrouch, colliderCentreCrouch;
	private bool _punching, _deflect, _slashing;
	private Vector3 colliderCentre;
	private Animator animator;
	private Coroutine crtCrouch, crtPunch, crtDeflect, crtSlash;
	new private CapsuleCollider collider;
	private Vector3 _velocity;
	Humanoid humanoid;

	//PROPERTIES

	//Movement
	public bool roll { set { if (value) animator.SetTrigger("roll"); } }
	public bool land { set { if (value) animator.SetTrigger("land"); } }
	public bool sliding { set => ResetRoutine(Crouch(value), ref crtCrouch); }
	public bool hanging { set => animator.SetBool("hanging", value); }
	public bool falling { set => animator.SetBool("falling", value); }

	//Attacks
	public bool holdingMelee { set; get; }
	public bool holdingPistol { set => animator.SetFloat("holdingPistol", value ? 1 : 0); }
	public bool shoot { set { if (value) animator.SetTrigger("shoot"); } }
	public bool slash
	{
		set { if (value) { _slashing = true; SetTrigger("slash", ref crtSlash, () => _slashing = false); } }
		get => _slashing;
	}
	public bool punch
	{
		set { if (value) { _punching = true; SetTrigger("punch", ref crtPunch, () => _punching = false); } }
		get => _punching;
	}
	public bool deflect
	{
		set { if (value) { _deflect = true; SetTrigger("deflect", ref crtDeflect, () => _deflect = false); } }
		get => _deflect;
	}

	//Other
	public bool dying { set => animator.SetBool("dying", value); }
	public bool grounded { set => falling = false; }
	public bool wallRunning { set => falling = false; }
	public Vector3 velocity
	{
		set
		{
			_velocity = value;
			float magnitude = Mathf.Sqrt((value.x * value.x) + (value.z * value.z));
			magnitude = ((int)(magnitude * 100)) / 100f;//round to 2 decimals
			animator.SetFloat("walkMagnitude", Mathf.InverseLerp(0, walkSpeed, magnitude));
			animator.SetFloat("runMagnitude", Mathf.InverseLerp(walkSpeed, runSpeed, magnitude));
			//animator.SetBool("falling", Mathf.Abs(value.y) >= airSpeed);
		}
	}

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		animator = GetComponent<Animator>();
		collider = transform.parent.GetComponent<CapsuleCollider>();
		colliderHeight = collider.height;
		colliderHeightCrouch = colliderHeight * crouchHeightMultiplier;
		colliderCentre = collider.center;
		colliderCentreCrouch = colliderCentre.y * crouchHeightMultiplier;
		humanoid = GetComponentInParent<Humanoid>();

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

	void SetTrigger(string name, ref Coroutine crt, Action onEnd)
	{
		ResetRoutine(WaitForEnd(), ref crt);
		IEnumerator WaitForEnd()
		{
			animator.SetTrigger(name);
			yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(name) || animator.GetNextAnimatorStateInfo(0).IsName(name));
			yield return new WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).IsName(name) || animator.GetNextAnimatorStateInfo(0).IsName(name));
			onEnd();
		}
	}

	public void PlayFootstepSound()
	{
		if (footstepSounds.Length == 0) return;
		// Pick random sound from array
		AudioClip footstepSound = footstepSounds[UnityEngine.Random.Range(0, footstepSounds.Length)];

		// Change the volume based on the player's velocity
		float velocityMagnitude = Mathf.Sqrt((_velocity.x * _velocity.x) + (_velocity.z * _velocity.z));//should fix so this works on ramps
		float volume = Mathf.Lerp(minStepVolume, maxStepVolume, velocityMagnitude / velocityAtMaxStepVolume);
		Sound.MakeSound(transform.position, footStepSoundRadius * velocityMagnitude, footstepSound, audioSource, volume, humanoid);
	}

	public void PlayJumpSound()
	{
		if (jumpingSounds.Length == 0) return;
		AudioClip jumpSound = jumpingSounds[UnityEngine.Random.Range(0, jumpingSounds.Length)];

		audioSource.PlayOneShot(jumpSound);
	}

	public void PlayLandingSound()
	{
		if (landingSounds.Length == 0) return;
		AudioClip landingSound = landingSounds[UnityEngine.Random.Range(0, landingSounds.Length)];

		audioSource.PlayOneShot(landingSound);
	}
}