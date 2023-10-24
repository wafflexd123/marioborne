using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioPool))]
public class HumanoidAnimatorManager : MonoBehaviourPlus
{
	//Inspector
	public AudioPool.Clips footstepSounds, jumpingSounds, landingSounds;
	public float walkSpeed, runSpeed, colliderCrouchTime, crouchHeightMultiplier, footStepSoundRadius, maxAdditionalStepVolume, velocityAtMaxStepVolume;
	public GameObject deathPosePrefab;

	//Script
	private float colliderHeight, colliderHeightCrouch, colliderCentreCrouch;
	private bool _punching, _deflect, _slashing, _jumpAttack, _backflip, _shieldLayer, _sniperLayer;
	private Vector3 colliderCentre;
	private Animator animator;
	private Coroutine crtCrouch, crtPunch, crtDeflect, crtSlash, crtJumpAttack, crtBackflip;
	[SerializeField] new private CapsuleCollider collider;
	private Vector3 _velocity;
	Humanoid humanoid;
	AudioPool audioPool;

	//PROPERTIES

	//Movement
	public bool roll { set { if (value) animator.SetTrigger("roll"); } }
	public bool land { set { if (value) animator.SetTrigger("land"); } }
	public bool sliding { set => ResetRoutine(Crouch(value), ref crtCrouch); }
	public bool hanging { set => animator.SetBool("hanging", value); }
	public bool falling { set => animator.SetBool("falling", value); }

	//Attacks
	public bool holdingMelee { set => animator.SetFloat("holdingMelee", value ? 1 : 0); }
	public bool holdingPistol { set => animator.SetFloat("holdingPistol", value ? 1 : 0); }
	public bool holdingShield { set => animator.SetFloat("holdingShield", value ? 1 : 0); }
	public bool holdingSniper { set => animator.SetFloat("holdingSniper", value ? 1 : 0); }
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
	public bool jumpAttack
	{
		set { if (value) { _jumpAttack = true; SetTrigger("jumpAttack", ref crtJumpAttack, () => _jumpAttack = false); } }
		get => _jumpAttack;
	}
	public bool backflip
	{
		set { if (value) { _backflip = true; SetTrigger("backflip", ref crtBackflip, () => _backflip = false); } }
		get => _backflip;
	}
	public bool shieldLayer
    {
		set { if (value) { animator.SetLayerWeight(2, 1); } }
		get => _shieldLayer;
	}
	public bool sniperLayer
	{
		set { if (value) { animator.SetLayerWeight(1, 1); } }
		get => _sniperLayer;
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
		float maxTime = footstepSounds.MaxShotLength();
		float temp = jumpingSounds.MaxShotLength();
		if (temp > maxTime) maxTime = temp;
		temp = landingSounds.MaxShotLength();
		if (temp > maxTime) maxTime = temp;

		audioPool = GetComponent<AudioPool>().Initialise(1f, maxTime);//assuming sounds wont overlap after 1 second
		animator = GetComponent<Animator>();
        collider = collider == null ? transform.parent.GetComponent<CapsuleCollider>() : collider;
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
		float velocityMagnitude = Mathf.Sqrt((_velocity.x * _velocity.x) + (_velocity.z * _velocity.z));//need to fix so this works on ramps
		footstepSounds.PlayRandom(audioPool, Mathf.Lerp(0, maxAdditionalStepVolume, velocityMagnitude / velocityAtMaxStepVolume));
		Sound.MakeSound(transform.position, footStepSoundRadius * velocityMagnitude, humanoid);
	}

	public void PlayJumpSound()
	{
		jumpingSounds.PlayRandom(audioPool);
	}

	public void PlayLandingSound()
	{
		landingSounds.PlayRandom(audioPool);
	}
}