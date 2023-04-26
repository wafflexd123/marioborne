using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPlus
{
	[Header("Movement")]
	public float minWalkForce;
	public float maxWalkForce, velocityAtMaxWalkForce;
	public AnimationCurve walkForceCurve;
	public float jumpForce, dashForce, dashCooldown, airMultiplier;

	[Header("Wall Running")]
	public float wallRunGravity;
	public float wallJumpForce, wallRunForce, wallTilt, tiltPerSecond, maxWallUpwardsVelocity, minWallRunVelocity;

	[Header("Drag")]
	public float minGroundDrag;
	public float maxGroundDrag, velocityAtMaxGroundDrag;
	public AnimationCurve groundDragCurve;
	public float airDrag = .8f, wallDrag = .8f;

	[Header("FOV")]
	public float velocityAtMaxFov;
	public float fovAtMaxVelocity, fovPerSecond;
	public AnimationCurve fovCurve;

	[Header("Layers")]
	public LayerMask layerGround;
	public LayerMask layerWall;

	//Private
	float mass, startFov, wallDistance = .6f, _tilt;
	bool queueJump, queueDash, canDoubleJump = true, isGrounded, isWallrunning;
	Vector3 moveDirection;
	new Rigidbody rigidbody;
	new Camera camera;
	PlayerCamera playerCamera;
	Wall wall = new Wall();
	Coroutine crtDash;
	Transform tfmBody, tfmGround, tfmSlope;
	Coroutine crtTilt;
	HumanoidAnimatorManager animator;
	Console.Line cnsDebug;

	//Properties
	public float currentTilt { get => _tilt; private set { _tilt = value; playerCamera.rotationOffset = new Vector3(0, 0, _tilt); } }

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
		if (Input.GetButtonDown("Dash")) queueDash = true;
		rigidbody.mass = mass / Time.timeScale;
	}

	void FixedUpdate()
	{
		moveDirection = (tfmBody.forward * Input.GetAxisRaw("Vertical") + tfmBody.right * Input.GetAxisRaw("Horizontal")).normalized;
		CheckContacts();
		ControlDrag();
		MovePlayer();
		Jump();
		Dash();
		ControlFOV();

		//Animation
		animator.velocity = rigidbody.velocity;

		//Console
		cnsDebug.text = $"Velocity: {rigidbody.velocity.magnitude:#.00} {rigidbody.velocity}, grounded: {isGrounded}, drag: {rigidbody.drag}\n" +
			$"On wall: {wall.IsOnWall}, direction: {wall.direction}, wallrunning: {isWallrunning}";
	}

	void ControlFOV()
	{
		float targetFov = Mathf.Lerp(startFov, fovAtMaxVelocity, fovCurve.Evaluate(Mathf.Clamp01(rigidbody.velocity.magnitude / velocityAtMaxFov)));
		camera.fieldOfView = TweenFloat(camera.fieldOfView, targetFov, fovPerSecond);
	}

	void CheckContacts()
	{
		//ground check
		foreach (Transform t in tfmGround)
		{
			if (Physics.CheckSphere(t.position, .01f, layerGround))
			{
				isGrounded = true;
				return;
			}
		}
		isGrounded = false;

		//Wall check
		if (Physics.Raycast(transform.position, tfmBody.right, out wall.hit, wallDistance, layerWall))
		{
			wall.direction = 1;
		}
		else
		{
			Physics.Raycast(transform.position, -tfmBody.right, out wall.hit, wallDistance, layerWall);
			wall.direction = -1;
		}
	}

	void MovePlayer()
	{
		if (isGrounded)//on ground
		{
			if (isWallrunning) WallRun(false);
			Physics.Raycast(tfmSlope.position + Vector3.up, Vector3.down, out RaycastHit slopeHit, 1.1f, layerGround + layerWall);
			rigidbody.AddForce(Mathf.Lerp(minWalkForce, maxWalkForce, walkForceCurve.Evaluate(Mathf.Clamp01(rigidbody.velocity.magnitude / velocityAtMaxWalkForce))) * Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized, ForceMode.Force);
		}
		else if (wall.IsOnWall && LateralVelocity() >= minWallRunVelocity)//on wall and going fast enough
		{
			if (!isWallrunning) WallRun(true);
			rigidbody.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);
			rigidbody.AddForce(Input.GetAxisRaw("Vertical") * wallRunForce * Vector3.ProjectOnPlane(tfmBody.forward, wall.hit.normal).normalized, ForceMode.Force);
		}
		else//in air
		{
			if (isWallrunning) WallRun(false);
			rigidbody.AddForce(airMultiplier * Mathf.Lerp(minWalkForce, maxWalkForce, walkForceCurve.Evaluate(Mathf.Clamp01(rigidbody.velocity.magnitude / velocityAtMaxWalkForce))) * moveDirection, ForceMode.Force);
		}
	}

	float LateralVelocity()
	{
		return Mathf.Sqrt((rigidbody.velocity.x * rigidbody.velocity.x) + (rigidbody.velocity.z * rigidbody.velocity.z));//mostly functional, doesnt account for any y velocity added because of ramps yet
	}

	void WallRun(bool enable)
	{
		isWallrunning = enable;
		rigidbody.useGravity = !enable;
		if (enable && rigidbody.velocity.y > maxWallUpwardsVelocity) rigidbody.velocity = new Vector3(rigidbody.velocity.x, maxWallUpwardsVelocity, rigidbody.velocity.z);//prevent player from going over wall when hitting it
		ResetRoutine(TweenFloat(() => currentTilt, (float tilt) => currentTilt = tilt, enable ? wallTilt * wall.direction : 0, tiltPerSecond), ref crtTilt);
		//ResetRoutine(LerpFloat(() => camera.fieldOfView, (float fov) => camera.fieldOfView = fov, fov, fovPerSecond), ref crtFOV); --not using wallrun fov rn
	}

	void Jump()
	{
		if (isGrounded) canDoubleJump = true;
		else if (isWallrunning) canDoubleJump = false;

		if (queueJump)
		{
			queueJump = false;
			if (isGrounded || isWallrunning)
			{
				Force();
			}
			else if (canDoubleJump)
			{
				Force();
				canDoubleJump = false;
			}
		}

        void Force()
		{
			Vector3 direction;
			float force;
			if (isWallrunning)
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
	}

	void Dash()
	{
		if (queueDash)
		{
			queueDash = false;
			if (crtDash == null && isGrounded) crtDash = StartCoroutine(Routine());
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
		if (isGrounded)
			rigidbody.drag = Mathf.Lerp(minGroundDrag, maxGroundDrag, groundDragCurve.Evaluate(Mathf.Clamp01(rigidbody.velocity.magnitude / velocityAtMaxGroundDrag)));
		else if (isWallrunning)
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
}
