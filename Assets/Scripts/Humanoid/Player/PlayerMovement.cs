using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPlus
{
	[Header("Walk/Run")]
	public ForceCurve walkForce;
	public float walkDrag;
	public ForceCurve noInputGroundDrag;
	public ForceCurve fovCurve;
	public float fovPerSecond;

	[Header("Air")]
	public ForceCurve airForce;
	public float airDrag, jumpForce, doubleJumpForce;

	[Header("Roll & Slide")]
	public float rollQueueTime;
	public float rollRequeueTime;
	public ForceCurve slideDrag;

	[Header("Wall Running")]
	public ForceCurve wallForce;
	public float wallRunGravity, wallJumpForce, wallTilt, tiltPerSecond, maxWallUpwardsVelocity, wallCatchDistance = .6f, wallDrag, wallCatchHeight;
	public Vector3 wallJumpAngle;

	[Header("Layers")]
	public LayerMask layerGround;
	public LayerMask layerWall;

	//Private
	float mass, _tilt;
	bool queueJump, queueDash, canDoubleJump, canWallJump, hitWall, _isGrounded, _isWallrunning, _isSliding, onLedge;
	Vector3 moveDirection;
	new Rigidbody rigidbody;
	new Camera camera;
	PlayerCamera playerCamera;
	Wall wallSide = new Wall(), wallForward = new Wall();
	Coroutine crtDash, crtTilt, crtSlide;
	Transform tfmBody, tfmGround, tfmSlope;
	HumanoidAnimatorManager animator;
	Console.Line cnsDebug;
	[HideInInspector] public LeapObject closestLeapObject;
	[HideInInspector] public CatchLedge closestCatchLedge;
	CatchLedge lastLedge;
	List<Force> forces = new List<Force>();

	//Temporary
	float dashForce, dashCooldown;

	//Properties
	public float CurrentTilt { get => _tilt; private set { _tilt = value; playerCamera.rotationOffset = new Vector3(0, 0, _tilt); } }
	public bool IsGrounded { get => _isGrounded; private set { _isGrounded = value; animator.grounded = value; } }
	public bool IsWallrunning { get => _isWallrunning; private set { _isWallrunning = value; animator.wallRunning = value; } }
	public bool IsSliding { get => _isSliding; private set { _isSliding = value; animator.sliding = value; } }

	void Start()
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
		cnsDebug = Console.AddLine();
		jumpForce *= rigidbody.mass;
		wallRunGravity *= rigidbody.mass;
		wallJumpForce *= rigidbody.mass;
		doubleJumpForce *= rigidbody.mass;
		walkForce.Multiply(rigidbody.mass);
		wallForce.Multiply(rigidbody.mass);
		airForce.Multiply(rigidbody.mass);
	}

	void Update()
	{
		if (Input.GetButtonDown("Jump")) queueJump = true;
		if (Input.GetButtonDown("Crouch") && !IsGrounded && !IsSliding) animator.QueueRoll(rollQueueTime, rollRequeueTime);
		if (Input.GetButtonDown("Dash")) queueDash = true;
		rigidbody.mass = mass / Time.timeScale;
	}

	void FixedUpdate()
	{
		//Input
		moveDirection = (tfmBody.forward * Input.GetAxis("Vertical") + tfmBody.right * Input.GetAxis("Horizontal")).normalized;

		//Control
		CheckContacts();
		CatchWall();
		MovePlayer();
		Jump();
		Dash();
		Slide();
		ControlFOV();

		//Interfacing
		for (int i = 0; i < forces.Count; i++) rigidbody.AddForce(forces[i].force, forces[i].forceMode);
		PrintForce();
		forces.Clear();
		animator.velocity = rigidbody.velocity;
	}

	void PrintForce()
	{
		if (Console.Enabled)
		{
			Vector3 totalForce = Vector3.zero;
			for (int i = 0; i < forces.Count; i++) if (forces[i].forceMode == ForceMode.Force) totalForce += forces[i].force;
			totalForce /= rigidbody.mass;
			ForceCurve curve = IsGrounded ? walkForce : IsWallrunning ? wallForce : airForce;
			float velocity = IsWallrunning ? LateralVelocity() : rigidbody.velocity.magnitude;
			cnsDebug.text = $"Force: {totalForce.magnitude:#.00} ({Mathf.InverseLerp(curve.minForce, curve.maxForce, curve.Evaluate(velocity)) * 100:#0}% of current curve) {totalForce}\n" +
				$"Drag: {rigidbody.drag}\n" +
				$"Grounded: {IsGrounded}, wallrunning: {IsWallrunning}, in air: {!IsGrounded && !IsWallrunning}, on wall: ({wallSide.IsTouchingWall}, direction: {wallSide.direction})\n" +
				$"Can doublejump: {canDoubleJump}, on ledge: {onLedge}";
		}
	}

	void Slide()
	{
		if (Input.GetButton("Crouch"))
		{
			if (onLedge)
			{
				onLedge = false;
				rigidbody.useGravity = true;
				canDoubleJump = false;
				animator.hanging = false;
			}
			else if (IsGrounded && crtSlide == null)
			{
				crtSlide = StartCoroutine(Routine());
			}
		}
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
		camera.fieldOfView = TweenFloat(camera.fieldOfView, fovCurve.Evaluate(LateralVelocity()), fovPerSecond);
	}

	void CheckContacts()
	{
		//Ground check
		foreach (Transform t in tfmGround)
		{
			if (Physics.CheckSphere(t.position, .01f, layerGround, QueryTriggerInteraction.Ignore))
			{
				if (!IsGrounded)
				{
					IsGrounded = true;
					lastLedge = null;
				}
				return;
			}
		}
		IsGrounded = false;

		//Wall checks
		if (!wallSide.CheckWall(transform, transform.right, wallCatchDistance, layerWall))//right
			wallSide.CheckWall(transform, -transform.right, wallCatchDistance, layerWall);//left
		wallForward.CheckWall(transform, transform.forward, wallCatchDistance, layerWall);//forward
	}

	void AddForce(Vector3 force, ForceMode forceMode)
	{
		forces.Add(new Force(force, forceMode));
	}

	void MovePlayer()
	{
		if (!onLedge)
		{
			if (IsGrounded)//on ground
			{
				if (IsWallrunning) WallRun(false);

				if (moveDirection == Vector3.zero) rigidbody.drag = noInputGroundDrag.Evaluate(LateralVelocity());
				else
				{
					if (IsSliding) rigidbody.drag = slideDrag.Evaluate(LateralVelocity());
					else rigidbody.drag = walkDrag;
					Physics.Raycast(tfmSlope.position + Vector3.up, Vector3.down, out RaycastHit slopeHit, 1.1f, layerGround + layerWall);
					AddForce(walkForce.Evaluate(rigidbody.velocity.magnitude, Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized), ForceMode.Force);
				}
			}
			else if (wallSide.IsTouchingWall && (!Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hitGround, 2f, layerGround) || Vector3.Distance(transform.position, hitGround.point) >= wallCatchHeight))//if touching a wall and the player has jumped high enough
			{
				if (!IsWallrunning) WallRun(true);
				rigidbody.drag = wallDrag;
				if (moveDirection != Vector3.zero) AddForce(wallForce.Evaluate(LateralVelocity(), Input.GetAxis("Vertical") * Vector3.ProjectOnPlane(tfmBody.forward, wallSide.hit.normal).normalized), ForceMode.Force);
			}
			else//in air
			{
				if (IsWallrunning) WallRun(false);
				rigidbody.drag = airDrag;
				if (moveDirection != Vector3.zero) AddForce(airForce.Evaluate(rigidbody.velocity.magnitude, moveDirection), ForceMode.Force);
			}
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
		ResetRoutine(TweenFloat(() => CurrentTilt, (float tilt) => CurrentTilt = tilt, enable ? wallTilt * wallSide.direction : 0, tiltPerSecond), ref crtTilt);
		//ResetRoutine(LerpFloat(() => camera.fieldOfView, (float fov) => camera.fieldOfView = fov, fov, fovPerSecond), ref crtFOV); --not using wallrun fov rn
	}

	void CatchWall()
	{
		if (!onLedge && closestCatchLedge != null && closestCatchLedge != lastLedge)
		{
			animator.hanging = true;
			onLedge = true;
			rigidbody.useGravity = false;
			rigidbody.velocity = Vector3.zero;
			lastLedge = closestCatchLedge;
			transform.position = new Vector3(transform.position.x, lastLedge.bodyPos.position.y, transform.position.z);//stupid
		}
	}

	void Jump()
	{
		if (queueJump)
		{
			queueJump = false;
			if (closestLeapObject != null && closestLeapObject.CanLeap(camera.transform))
			{
				Force(closestLeapObject.GetLeapForce(rigidbody.mass));
				canDoubleJump = false;
			}
			else
			{
				if (IsGrounded)
				{
					Force(tfmBody.up * jumpForce);
					canDoubleJump = false;
				}
				else if (IsWallrunning)
				{
					Force((tfmBody.up + wallSide.hit.normal + wallJumpAngle).normalized * wallJumpForce);
					canDoubleJump = true;//only double jump if we jumped off a wall
				}
				else if (canDoubleJump)
				{
					Force(tfmBody.up * doubleJumpForce);
					canDoubleJump = false;
				}
				else if (onLedge)
				{
					onLedge = false;
					rigidbody.useGravity = true;
					Force(tfmBody.up * jumpForce);
					canDoubleJump = false;
					animator.hanging = false;
				}
			}
		}

		if (canWallJump && hitWall)
		{
			Force(tfmBody.up * jumpForce);
			canWallJump = false;
		}

		void Force(Vector3 force)
		{
			rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
			AddForce(force, ForceMode.Impulse);
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
			AddForce(moveDirection * dashForce, ForceMode.Impulse);
			yield return new WaitForSeconds(dashCooldown);
			crtDash = null;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		animator.Collide(rigidbody.velocity);
	}

	class Wall
	{
		public RaycastHit hit;
		public int direction;
		public bool IsTouchingWall => hit.transform != null;

		public bool CheckWall(Transform body, Vector3 bodyDirection, float distance, LayerMask layer, int direction = 1)
		{
			this.direction = direction;
			return Physics.Raycast(body.position, bodyDirection, out hit, distance, layer);
		}
	}

	[System.Serializable]
	public class ForceCurve
	{
		public AnimationCurve curve;
		public float minForce, maxForce, velocityAtMaxForce;

		public Vector3 Evaluate(float currentVelocity, Vector3 direction)
		{
			return Mathf.Lerp(minForce, maxForce, curve.Evaluate(Mathf.Clamp01(currentVelocity / velocityAtMaxForce))) * direction;
		}

		public float Evaluate(float currentVelocity)
		{
			return Mathf.Lerp(minForce, maxForce, curve.Evaluate(Mathf.Clamp01(currentVelocity / velocityAtMaxForce)));
		}

		public void Multiply(float multiplier)
		{
			minForce *= multiplier;
			maxForce *= multiplier;
		}
	}

	public struct Force
	{
		public Vector3 force;
		public ForceMode forceMode;

		public Force(Vector3 force, ForceMode forceMode)
		{
			this.force = force;
			this.forceMode = forceMode;
		}
	}
}
