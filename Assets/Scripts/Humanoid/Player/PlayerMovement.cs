using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPlus
{
	[Header("Walk/Run")]
	public ForceCurve walkForce;
	public float walkDrag;
	public ForceCurve noInputGroundDrag;
	public ForceCurve fovCurve;
	public float fovPerSecond;

	[Header("Air/Jump")]
	public ForceCurve airForce;
	public float airDrag, jumpForce, doubleJumpForce, ledgeJumpForce;
	public Vector3 gravity;
	public bool useGravity;

	[Header("Roll & Slide/Crouch")]
	public float rollQueueTime;
	public float rollRequeueTime, fallRollEngageSpeed, fallCrouchEngageSpeed;
	public ForceCurve slideDrag;

	[Header("Fall Damage")]
	public float minDamageVelocity;
	public float maxDamageVelocity, maxHealth, healthRecoverDelay, healthPerSecond;

	[Header("Wall Running")]
	public ForceCurve wallForce;
	public float wallRunGravity, wallJumpForce, wallTilt, tiltPerSecond, maxWallUpwardsVelocity, wallCatchDistance = .6f, wallDrag, wallCatchHeight;
	public Vector3 wallJumpAngle;

	[Header("Collisions")]
	public LayerMask layerGround;
	public LayerMask layerWall;
	public Vector3 groundCheckOffset;

	//Private
	float mass, _tilt, currentDrag, health;
	bool queueJump, queueDash, canDoubleJump, canWallJump, hitWall, _isGrounded, _isWallrunning, _isSliding, _onLedge, allowMovement = true, queueRoll;
	Vector3 moveDirection, velocity;
	RaycastHit groundHit;
	new Rigidbody rigidbody;
	new Camera camera;
	new CapsuleCollider collider;
	PlayerCamera playerCamera;
	Player player;
	Wall wallSide = new Wall(), wallForward = new Wall();
	Coroutine crtDash, crtTilt, crtSlide, crtQueueRoll, crtHealth;
	HumanoidAnimatorManager animator;
	Console.Line cnsDebug;
	CatchLedge lastLedge;
	TMP_Text txtWhereAmI;
	VignetteControl healthVignette;
	List<Force> forces = new List<Force>();
	[HideInInspector] public LeapObject closestLeapObject;
	[HideInInspector] public CatchLedge closestCatchLedge;

	//Temporary
	float dashForce, dashCooldown, lastFallVelocity;

	//Properties
	public float CurrentTilt { get => _tilt; private set { _tilt = value; playerCamera.rotationOffset = new Vector3(0, 0, _tilt); } }
	public bool IsGrounded { get => _isGrounded; private set { _isGrounded = value; animator.grounded = value; } }
	public bool IsWallrunning { get => _isWallrunning; private set { _isWallrunning = value; animator.wallRunning = value; } }
	public bool IsSliding { get => _isSliding; private set { _isSliding = value; animator.sliding = value; } }
	bool OnLedge { get => _onLedge; set { _onLedge = value; animator.hanging = value; rigidbody.useGravity = !value; } }

	IEnumerator Start()
	{
		health = maxHealth;
		player = GetComponent<Player>();
		collider = transform.Find("Body").GetComponent<CapsuleCollider>();
		animator = collider.transform.Find("Model").GetComponent<HumanoidAnimatorManager>();
		Transform ui = transform.Find("UI");
		txtWhereAmI = ui.Find("WhereAmI").GetComponent<TMP_Text>();
		txtWhereAmI.text = "";
		healthVignette = ui.Find("Health Vignette").GetComponent<VignetteControl>();
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.freezeRotation = true;
		rigidbody.useGravity = false;
		rigidbody.drag = 0;
		rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		rigidbody.excludeLayers = 1 << 12;
		rigidbody.includeLayers = ~rigidbody.excludeLayers;
		mass = rigidbody.mass;
		playerCamera = transform.Find("Head").GetComponent<PlayerCamera>();
		camera = playerCamera.transform.Find("Eyes").Find("Camera").GetComponent<Camera>();
		cnsDebug = Console.AddLine();
		if (useGravity)
		{
			useGravity = false;
			yield return new WaitForFixedUpdate();//bugs out for some reason if gravity is enabled on first frame
			useGravity = true;
		}
	}

	void Update()
	{
		if (Input.GetButtonDown("Jump")) queueJump = true;
		if (Input.GetButtonDown("Crouch") && !IsGrounded && !IsSliding) QueueRoll();
		if (Input.GetButtonDown("Dash")) queueDash = true;
	}

	void FixedUpdate()
	{
		//Input
		moveDirection = (collider.transform.forward * Input.GetAxis("Vertical") + collider.transform.right * Input.GetAxis("Horizontal")).normalized;

		//Control
		CheckGround();
		CheckWalls();
		CatchWallLedge();
		if (allowMovement)
		{
			MovePlayer();
			Jump();
			Dash();
			Slide();
		}
		ControlRigidbody();
		ControlFOV();
		ControlText();
	}

	/// <summary>
	/// Will let a roll initiate when you hit the ground if the button was pressed during queueTime, will not let a roll initiate afterwards during requeueTime or until the frame after you hit the ground
	/// </summary>
	public void QueueRoll()
	{
		if (crtQueueRoll == null) crtQueueRoll = StartCoroutine(Routine());
		else queueRoll = false;//player cannot spam roll button, must only press once
		IEnumerator Routine()
		{
			queueRoll = true;
			yield return new WaitForSeconds(rollQueueTime);
			queueRoll = false;
			yield return new WaitForSeconds(rollRequeueTime);
			crtQueueRoll = null;
		}
	}

	void Health(float amount, DeathType deathType)
	{
		if (amount > 0)
		{
			health -= amount;
			healthVignette.SetVignetteAlpha(health, maxHealth);
			if (health <= 0) player.Kill(deathType);
			else ResetRoutine(Recover(), ref crtHealth);
		}

		IEnumerator Recover()
		{
			yield return new WaitForSeconds(healthRecoverDelay);
			do
			{
				health += healthPerSecond * Time.fixedDeltaTime;
				healthVignette.SetVignetteAlpha(health, maxHealth);
				yield return new WaitForFixedUpdate();
			} while (health < maxHealth);
			health = maxHealth;
			crtHealth = null;
		}
	}

	void Collide()
	{
		if (rigidbody.velocity.y > velocity.y)//rigidbody y velocity will differ from intended velocity if a ground collision occurred, but will not always equal 0 due to interpolation (i think)
		{
			CheckGround();//this will call twice a frame but cant be bothered
			if (IsGrounded)
			{
				if (queueRoll && velocity.y <= -fallRollEngageSpeed)//if queueing a roll & hit the ground at roll speed
				{
					animator.roll = true;
					queueRoll = false;
					StopCoroutine(crtQueueRoll);
					crtQueueRoll = null;

					lastFallVelocity = -velocity.y;//store y value as positive
					velocity.y = 0;
					velocity = Vector3.ClampMagnitude(velocity + (lastFallVelocity * moveDirection), walkForce.maxForce);//add y velocity to forward velocity in direction of movement, dont go faster than maxForce
					rigidbody.velocity = velocity;//apply velocity change
					return;//don't bother setting velocity again
				}
				else if (velocity.y <= -fallCrouchEngageSpeed)//if hit the ground at crouch speed
				{
					animator.land = true;
					if (crtQueueRoll != null)//if a queued roll is currently waiting for requeueTime, re-enable rolls
					{
						queueRoll = false;
						StopCoroutine(crtQueueRoll);
						crtQueueRoll = null;
					}

					lastFallVelocity = -velocity.y;
					velocity.y = 0;
					float velocityPercent = Mathf.InverseLerp(minDamageVelocity, maxDamageVelocity, lastFallVelocity);
					velocity *= 1 - velocityPercent;//reduce velocity according to percent of health decreased
					Health(Mathf.Lerp(0, maxHealth, velocityPercent), DeathType.Fall);
					rigidbody.velocity = velocity;//apply velocity change
					return;//don't bother setting velocity again
				}
			}
		}

		velocity = rigidbody.velocity;//account for collisions
	}

	void ControlText()
	{
		if (wallSide.hit.transform != null) txtWhereAmI.text = wallSide.hit.transform.name;
		else if (IsGrounded && groundHit.transform != null) txtWhereAmI.text = groundHit.transform.name;
	}

	void ControlRigidbody()
	{
		for (int i = 0; i < forces.Count; i++)
		{
			switch (forces[i].forceMode)
			{
				case ForceMode.Force:
					velocity += forces[i].force / mass * Time.fixedDeltaTime;
					break;
				case ForceMode.Acceleration:
					velocity += forces[i].force * Time.fixedDeltaTime;
					break;
				case ForceMode.Impulse:
					velocity += forces[i].force / mass;
					break;
				case ForceMode.VelocityChange:
					velocity += forces[i].force;
					break;
			}
		}
		PrintForce();
		forces.Clear();
		animator.velocity = velocity;

		if (!IsGrounded && useGravity) velocity += gravity * Time.fixedDeltaTime;

		velocity *= 1 - Time.fixedDeltaTime * currentDrag;
		//Debug.Log($"{currentDrag}   {1 - Time.fixedDeltaTime * currentDrag}   {(1 * Time.timeScale) - Time.fixedUnscaledDeltaTime * currentDrag}");

		rigidbody.velocity = velocity * Time.timeScale;
	}

	void PrintForce()
	{
		if (Console.Enabled)
		{
			ForceCurve curve = IsGrounded ? walkForce : IsWallrunning ? wallForce : airForce;
			float velocity = IsWallrunning ? LateralVelocity() : this.velocity.magnitude;
			cnsDebug.text = $"Force: {this.velocity.magnitude:#.00} {this.velocity} ({Mathf.InverseLerp(curve.minForce, curve.maxForce, curve.Evaluate(velocity)) * 100:#0}% of current curve), actual velocity: {rigidbody.velocity.magnitude:#.00} {rigidbody.velocity}\n" +
				$"Drag: {currentDrag}\n" +
				$"Grounded: {IsGrounded}, last object walked on: {(groundHit.transform != null ? groundHit.transform.name : "not found")}\n" +
				$"Wallrunning: {IsWallrunning}, in air: {!IsGrounded && !IsWallrunning}, on wall: ({wallSide.IsTouchingWall}, direction: {wallSide.direction})\n" +
				$"Can doublejump: {canDoubleJump}, on ledge: {OnLedge}\n" +
				$"Health: {health:#0.00}, last fall velocity: {lastFallVelocity:#0.00}";
		}
	}

	void Slide()
	{
		if (Input.GetButton("Crouch"))
		{
			if (OnLedge)
			{
				OnLedge = false;
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

	void CheckGround()
	{
		if (Physics.CheckSphere(collider.transform.position + groundCheckOffset + new Vector3(0, collider.radius), collider.radius, layerGround, QueryTriggerInteraction.Ignore))
		{
			if (!IsGrounded)
			{
				IsGrounded = true;
				lastLedge = null;
			}
		}
		else IsGrounded = false;
	}

	void CheckWalls()
	{
		if (!wallSide.CheckWall(collider.transform, collider.transform.right, wallCatchDistance, layerWall))//right
			wallSide.CheckWall(collider.transform, -collider.transform.right, wallCatchDistance, layerWall, -1);//left
		wallForward.CheckWall(collider.transform, collider.transform.forward, wallCatchDistance, layerWall);//forward
	}

	void AddForce(Vector3 force, ForceMode forceMode)
	{
		forces.Add(new Force(force, forceMode));
	}

	void MovePlayer()
	{
		if (!OnLedge)
		{
			if (IsGrounded)//on ground
			{
				if (IsWallrunning) WallRun(false);

				if (moveDirection == Vector3.zero) currentDrag = noInputGroundDrag.Evaluate(LateralVelocity());
				else
				{
					if (IsSliding) currentDrag = slideDrag.Evaluate(LateralVelocity());
					else currentDrag = walkDrag;
					Physics.Raycast(collider.transform.position + Vector3.up, Vector3.down, out groundHit, 2f, layerGround + layerWall);
					AddForce(walkForce.Evaluate(velocity.magnitude, Vector3.ProjectOnPlane(moveDirection, groundHit.normal).normalized), ForceMode.Acceleration);
				}
			}
			else if (wallSide.IsTouchingWall && (!Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hitGround, 2f, layerGround) || Vector3.Distance(transform.position, hitGround.point) >= wallCatchHeight))//if touching a wall and the player has jumped high enough
			{
				if (!IsWallrunning) WallRun(true);
				currentDrag = wallDrag;
				if (moveDirection != Vector3.zero) AddForce(wallForce.Evaluate(LateralVelocity(), Input.GetAxis("Vertical") * Vector3.ProjectOnPlane(collider.transform.forward, wallSide.hit.normal).normalized), ForceMode.Acceleration);
				AddForce(new Vector3(0, -wallRunGravity), ForceMode.Acceleration);
			}
			else//in air
			{
				if (IsWallrunning) WallRun(false);
				currentDrag = airDrag;
				if (moveDirection != Vector3.zero) AddForce(airForce.Evaluate(velocity.magnitude, moveDirection), ForceMode.Acceleration);
			}
		}
	}

	float LateralVelocity()
	{
		return Mathf.Sqrt((velocity.x * velocity.x) + (velocity.z * velocity.z));//mostly functional, doesnt account for any y velocity added because of ramps yet
	}

	void WallRun(bool enable)
	{
		IsWallrunning = enable;
		useGravity = !enable;
		if (enable)
		{
			canDoubleJump = true;
			if (velocity.y > maxWallUpwardsVelocity)
				velocity = new Vector3(velocity.x, maxWallUpwardsVelocity, velocity.z);//prevent player from going over wall when hitting it
		}
		ResetRoutine(TweenFloat(() => CurrentTilt, (float tilt) => CurrentTilt = tilt, enable ? wallTilt * wallSide.direction : 0, tiltPerSecond), ref crtTilt);
		//ResetRoutine(LerpFloat(() => camera.fieldOfView, (float fov) => camera.fieldOfView = fov, fov, fovPerSecond), ref crtFOV); --not using wallrun fov rn
	}

	void CatchWallLedge()
	{
		if (!OnLedge && !IsGrounded && closestCatchLedge != null && closestCatchLedge != lastLedge)
		{
			OnLedge = true;
			velocity = Vector3.zero;
			lastLedge = closestCatchLedge;
			allowMovement = false;
			rigidbody.isKinematic = true;
			StartCoroutine(LerpToPos(transform, new Position(transform.position.x, lastLedge.bodyPos.position.y, transform.position.z), 5, () => allowMovement = !(rigidbody.isKinematic = false)));
		}
	}

	void Jump()
	{
		if (queueJump)
		{
			queueJump = false;
			if (closestLeapObject != null && closestLeapObject.CanLeap(camera.transform))
			{
				Force(closestLeapObject.GetLeapForce(1));
				canDoubleJump = false;
			}
			else
			{
				if (OnLedge)
				{
					OnLedge = false;
					Force(collider.transform.up * ledgeJumpForce);
					canDoubleJump = false;
				}
				if (IsGrounded)
				{
					Force(collider.transform.up * jumpForce);
					canDoubleJump = false;
				}
				else if (IsWallrunning)
				{
					Force((collider.transform.up + wallSide.hit.normal + wallJumpAngle).normalized * wallJumpForce);
				}
				else if (canDoubleJump)
				{
					Force(collider.transform.up * doubleJumpForce);
					canDoubleJump = false;
				}
			}
		}

		if (canWallJump && hitWall)
		{
			Force(collider.transform.up * jumpForce);
			canWallJump = false;
		}

		void Force(Vector3 force)
		{
			velocity = new Vector3(velocity.x, 0f, velocity.z);
			AddForce(force, ForceMode.VelocityChange);
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
			AddForce(moveDirection * dashForce, ForceMode.VelocityChange);
			yield return new WaitForSeconds(dashCooldown);
			crtDash = null;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		Collide();
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		if (collider == null) collider = transform.Find("Body").GetComponent<CapsuleCollider>();
		Gizmos.DrawWireSphere(collider.transform.position + groundCheckOffset + new Vector3(0, collider.radius), collider.radius);
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
