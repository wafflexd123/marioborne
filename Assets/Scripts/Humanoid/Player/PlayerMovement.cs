using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPlus
{
	[Header("Walk/Run")]
	public float minWalkForce;
	public float maxWalkForce, velocityAtMaxWalkForce;
	public AnimationCurve walkForceCurve;
	public float airMultiplier;

	[Header("Jump, Roll & Dash")]
	public float rollQueueTime;
	public float rollRequeueTime, jumpForce, dashForce, dashCooldown;

	[Header("Wall Running")]
	public float wallRunGravity;
	public float wallJumpForce, wallRunForce, wallTilt, tiltPerSecond, maxWallUpwardsVelocity, minWallRunLateralVelocity, wallCatchDistance = .6f;

	[Header("Drag")]
	public float minGroundDrag;
	public float maxGroundDrag, velocityAtMaxGroundDrag;
	public AnimationCurve groundDragCurve;
	public float airDrag = .8f, wallDrag = .8f, slideDrag;

	[Header("FOV")]
	public float velocityAtMaxFov;
	public float fovAtMaxVelocity, fovPerSecond;
	public AnimationCurve fovCurve;

	[Header("Layers")]
	public LayerMask layerGround;
	public LayerMask layerWall;

	//Private
	float mass, startFov, wallJumpDistance = 0.3f, wallJumpDelay = .1f, _tilt;
	bool queueJump, queueDash, canDoubleJump = true, canWallJump, hitWall, _isGrounded, _isWallrunning, _isSliding;
	Vector3 moveDirection;
	new Rigidbody rigidbody;
	new Camera camera;
	PlayerCamera playerCamera;
	Wall wall = new Wall();
	Coroutine crtDash, crtTilt, crtWallJump, crtSlide;
	Transform tfmBody, tfmGround, tfmSlope;
	HumanoidAnimatorManager animator;
	Console.Line cnsDebug;

	//Properties
	public float CurrentTilt { get => _tilt; private set { _tilt = value; playerCamera.rotationOffset = new Vector3(0, 0, _tilt); } }
	public bool IsGrounded { get => _isGrounded; private set { _isGrounded = value; animator.grounded = value; } }
	public bool IsWallrunning { get => _isWallrunning; private set { _isWallrunning = value; animator.wallRunning = value; } }
	public bool IsSliding { get => _isSliding; private set { _isSliding = value; animator.sliding = value; } }

	private void Start()
	{
		tfmBody = transform.Find("Body");
		tfmGround = tfmBody.Find("Ground Detection");
		tfmSlope = tfmBody.Find("Slope Detection");
		animator = tfmBody.Find("Model").GetComponent<HumanoidAnimatorManager>();
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.freezeRotation = true;
		mass = rigidbody.mass;
		playerCamera = transform.Find("Head").GetComponent<PlayerCamera>();
		camera = playerCamera.transform.Find("Eyes").Find("Camera").GetComponent<Camera>();
		startFov = camera.fieldOfView;
		cnsDebug = Console.AddLine();
	}

	void Update()
	{
		if (Input.GetButtonDown("Jump")) queueJump = true;
		if (Input.GetButtonDown("Crouch") && !IsGrounded && !IsSliding) animator.QueueRoll(rollQueueTime, rollRequeueTime);
		if (Input.GetButtonDown("Dash")) queueDash = true;
		rigidbody.mass = mass / Time.timeScale;
		CheckWall();
	}

	void FixedUpdate()
	{
		moveDirection = (tfmBody.forward * Input.GetAxisRaw("Vertical") + tfmBody.right * Input.GetAxisRaw("Horizontal")).normalized;
		CheckContacts();
		ControlDrag();
		MovePlayer();
		Jump();
		Dash();
		Slide();
		ControlFOV();

		//Animation
		animator.velocity = rigidbody.velocity;

		//Console
		cnsDebug.text = $"Velocity: {rigidbody.velocity.magnitude:#.00} {rigidbody.velocity}, grounded: {IsGrounded}, drag: {rigidbody.drag}\n" +
			$"On wall: {wall.IsOnWall}, direction: {wall.direction}, wallrunning: {IsWallrunning}";
	}

	void Slide()
	{
		if (Input.GetButton("Crouch") && IsGrounded && crtSlide == null) crtSlide = StartCoroutine(Routine());
		IEnumerator Routine()
		{
			IsSliding = true;
			do yield return new WaitForFixedUpdate();
			while (Input.GetButton("Crouch") && IsGrounded);
			IsSliding = false;
			crtSlide = null;
		}
	}

	void ControlFOV()
	{
		float targetFov = Mathf.Lerp(startFov, fovAtMaxVelocity, fovCurve.Evaluate(Mathf.Clamp01(LateralVelocity() / velocityAtMaxFov)));
		camera.fieldOfView = TweenFloat(camera.fieldOfView, targetFov, fovPerSecond);
	}

	void CheckContacts()
	{
		//ground check
		foreach (Transform t in tfmGround)
		{
			if (Physics.CheckSphere(t.position, .01f, layerGround))
			{
				if (!IsGrounded) IsGrounded = true;
				return;
			}
		}
		IsGrounded = false;

		//Wall check
		if (Physics.Raycast(transform.position, tfmBody.right, out wall.hit, wallCatchDistance, layerWall))
		{
			wall.direction = 1;
		}
		else
		{
			Physics.Raycast(transform.position, -tfmBody.right, out wall.hit, wallCatchDistance, layerWall);
			wall.direction = -1;
		}
	}

	void MovePlayer()
	{
		if (IsGrounded)//on ground
		{
			if (IsWallrunning) WallRun(false);
			Physics.Raycast(tfmSlope.position + Vector3.up, Vector3.down, out RaycastHit slopeHit, 1.1f, layerGround + layerWall);
			rigidbody.AddForce(Mathf.Lerp(minWalkForce, maxWalkForce, walkForceCurve.Evaluate(Mathf.Clamp01(rigidbody.velocity.magnitude / velocityAtMaxWalkForce))) * Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized, ForceMode.Force);
		}
		else if (wall.IsOnWall && (LateralVelocity() >= minWallRunLateralVelocity || Mathf.Abs(rigidbody.velocity.y) > jumpForce))//on wall and going fast enough in lateral OR y direction
		{
			if (!IsWallrunning) WallRun(true);
			rigidbody.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);
			rigidbody.AddForce(Input.GetAxisRaw("Vertical") * wallRunForce * Vector3.ProjectOnPlane(tfmBody.forward, wall.hit.normal).normalized, ForceMode.Force);
		}
		else//in air
		{
			if (IsWallrunning) WallRun(false);
			rigidbody.AddForce(airMultiplier * Mathf.Lerp(minWalkForce, maxWalkForce, walkForceCurve.Evaluate(Mathf.Clamp01(rigidbody.velocity.magnitude / velocityAtMaxWalkForce))) * moveDirection, ForceMode.Force);
		}
	}

	float LateralVelocity()
	{
		return Mathf.Sqrt((rigidbody.velocity.x * rigidbody.velocity.x) + (rigidbody.velocity.z * rigidbody.velocity.z));//mostly functional, doesnt account for any y velocity added because of ramps yet
	}

	void WallRun(bool enable)
	{
		IsWallrunning = enable;
		rigidbody.useGravity = !enable;
		if (enable && rigidbody.velocity.y > maxWallUpwardsVelocity) rigidbody.velocity = new Vector3(rigidbody.velocity.x, maxWallUpwardsVelocity, rigidbody.velocity.z);//prevent player from going over wall when hitting it
		ResetRoutine(TweenFloat(() => CurrentTilt, (float tilt) => CurrentTilt = tilt, enable ? wallTilt * wall.direction : 0, tiltPerSecond), ref crtTilt);
		//ResetRoutine(LerpFloat(() => camera.fieldOfView, (float fov) => camera.fieldOfView = fov, fov, fovPerSecond), ref crtFOV); --not using wallrun fov rn
	}

	void Jump()
	{
		if (IsGrounded)
		{
			canDoubleJump = true;
			canWallJump = false;
			if (crtWallJump == null && !IsWallrunning) crtWallJump = StartCoroutine(Routine());
		}
		else if (IsWallrunning) canDoubleJump = false;

		if (queueJump)
		{
			queueJump = false;
			if (IsGrounded || IsWallrunning)
			{
				Force();
			}
			else if (canDoubleJump)
			{
				Force();
				canDoubleJump = false;
			}
		}

		if (canWallJump)
		{
			if (hitWall)
			{
				Force();
				canWallJump = false;
			}
		}

		void Force()
		{
			Vector3 direction;
			float force;
			if (IsWallrunning)
			{
				direction = transform.up + wall.hit.normal;
				force = wallJumpForce;
			}
			else
			{
				direction = transform.up;
				force = jumpForce;
			}
			rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
			rigidbody.AddForce(direction * force, ForceMode.VelocityChange);
		}

		IEnumerator Routine()
		{
			yield return new WaitForSeconds(wallJumpDelay);
			canWallJump = true;
			crtWallJump = null;
		}
	}

	void Dash()
	{
		if (queueDash)
		{
			queueDash = false;
			if (crtDash == null && IsGrounded) crtDash = StartCoroutine(Routine());
		}

		IEnumerator Routine()
		{
			rigidbody.AddForce(moveDirection * dashForce, ForceMode.Impulse);
			yield return new WaitForSeconds(dashCooldown);
			crtDash = null;
		}
	}

	void ControlDrag()
	{
		if (IsGrounded)
		{
			if (IsSliding) rigidbody.drag = slideDrag;
			else rigidbody.drag = Mathf.Lerp(minGroundDrag, maxGroundDrag, groundDragCurve.Evaluate(Mathf.Clamp01(rigidbody.velocity.magnitude / velocityAtMaxGroundDrag)));
		}
		else if (IsWallrunning)
			rigidbody.drag = wallDrag;
		else
			rigidbody.drag = airDrag;
	}

	class Wall
	{
		public RaycastHit hit;
		public int direction;
		public bool IsOnWall => hit.transform != null;
	}

	void CheckWall()
	{
		hitWall = Physics.Raycast(transform.position + transform.up, tfmBody.transform.forward, wallJumpDistance);
	}

	private void OnCollisionEnter(Collision collision)
	{
		animator.Collide(rigidbody.velocity);
	}
}
