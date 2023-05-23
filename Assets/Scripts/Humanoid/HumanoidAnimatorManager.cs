using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HumanoidAnimatorManager : MonoBehaviourPlus
{
    // Add your footstep sounds here
    public AudioClip[] footstepSounds;
    public AudioClip[] jumpingSounds;
    public AudioClip[] landingSounds;

    public float walkSpeed, runSpeed, colliderCrouchTime, crouchHeightMultiplier;

    private AudioSource audioSource;
    private float colliderHeight, colliderHeightCrouch, colliderCentreCrouch;
    private bool _wallRunning, _grounded;
    private Vector3 colliderCentre;
    private Animator animator;
    private Coroutine crtGroundState, crtCrouch;
    new private CapsuleCollider collider;
	private Vector3 _velocity;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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

	public bool roll { set => animator.SetTrigger("roll"); }
	public bool land { set => animator.SetTrigger("land"); }
	public bool holdingMelee { set => animator.SetLayerWeight(1, value ? 1 : 0); }
    public bool holdingGun { set => animator.SetLayerWeight(2, value ? 1 : 0); }
    public bool punching { set => animator.SetBool("punching", value); }
    public bool melee { set => animator.SetBool("melee", value); }
    public bool shooting { set => animator.SetBool("shooting", value); }
    public bool deflect { set => animator.SetBool("deflect", value); }
	public bool dying { set => animator.SetBool("dying", value); }
	public bool grounded { set { _grounded = value; CheckGroundState(); } }
	public bool wallRunning { set { _wallRunning = value; CheckGroundState(); } }
	public bool sliding { set => ResetRoutine(Crouch(value), ref crtCrouch); }
	public bool hanging { set => animator.SetBool("hanging", value); }
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
			animator.SetFloat("timeScale", Time.timeScale);
		}
	}

    public void PlayFootstepSound()
    {
        if (footstepSounds.Length == 0) return; 
		// Pick random sound from array
        AudioClip footstepSound = footstepSounds[Random.Range(0, footstepSounds.Length)];

        // Change the volume based on the player's velocity
        float volume = Mathf.Sqrt((_velocity.x * _velocity.x) + (_velocity.z * _velocity.z)) / 10.0f; // Adjust if you want (Probably necessary but I dunno)
        volume = Mathf.Clamp(volume, 0.1f, 1.0f);

        audioSource.PlayOneShot(footstepSound, volume);
    }

	public void PlayJumpSound()
	{
        if (jumpingSounds.Length == 0) return;
        AudioClip jumpSound = jumpingSounds[Random.Range(0, jumpingSounds.Length)];

        audioSource.PlayOneShot(jumpSound);
    }

	public void PlayLandingSound()
	{
        if (landingSounds.Length == 0) return;
		AudioClip landingSound = landingSounds[Random.Range(0, landingSounds.Length)];

        audioSource.PlayOneShot(landingSound);
    }
}