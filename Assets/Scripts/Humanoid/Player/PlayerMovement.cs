using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPlus
{
	#region Variables
	[Header("Walk/Run")]
	[Tooltip("Input force to apply to rigidbody when walking/running, where x=magnitude of velocity and y=magnitude of force")]
	public ForceCurve walkForce;
	public float walkDrag;
	[Tooltip("Drag curve, where x=magnitude of velocity and y=magnitude of drag, when no movement input is registered. This can be used to slow you down quickly at low velocity (so you're not on ice), but keep you moving when at higher velocities.")]
	public ForceCurve noInputGroundDrag;
	public ForceCurve fovCurve;
	public float fovPerSecond;

	[Header("Air/Jump")]
	[Tooltip("Input force to apply to rigidbody when in air, where x=magnitude of velocity and y=magnitude of force")]
	public ForceCurve airForce;
	public float airDrag, jumpForce, doubleJumpForce;
	[Tooltip("Player uses gravity independent of other physics objects")]
	public Vector3 gravity;
	[Tooltip("Do not use rigidbody.gravity, use this!")]
	public bool useGravity;

	[Header("Roll & Slide/Crouch")]
	[Tooltip("This curve is used to interpolate between high velocity sliding and slow velocity crouch walking, where x=magnitude of velocity and y=magnitude of drag")]
	public ForceCurve slideDrag;
	[Tooltip("Multiplier for the amount a slope's steepness affects sliding acceleration")]
	public float slopeDragMultiplier;
	[Tooltip("How many seconds before you hit the ground that a roll can be registered")]
	public float rollQueueTime;
	[Tooltip("How many seconds, after rollQueueTime, before allowing a roll again (stops player from pressing the roll button too early)")]
	public float rollRequeueTime;
	[Tooltip("Minimum distance fallen at which rolls can occur")]
	public float fallRollEngageDistance;
	[Tooltip("Minimum distance fallen at which the fall-crouch animation can occur")]
	public float fallCrouchEngageDistance;

	[Header("Wall Running")]
	[Tooltip("Input force to apply to rigidbody when wall running, where x=magnitude of velocity and y=magnitude of force")]
	public ForceCurve wallForce;
	public float wallDrag, wallRunGravity, wallJumpForce, wallTilt, tiltPerSecond;
	[Tooltip("Y velocity is clamped to this value when entering a wall run; helps prevent player from going over walls when hitting them")]
	public float maxWallUpwardsVelocity;
	[Tooltip("Distance from centre of player collider that walls are registered as being touched")]
	public float wallCatchDistance = .6f;
	[Tooltip("How high you have to be before grabbing a wall (means you have to jump onto a wall to wallrun/walljump, instead of just walk into it)")]
	public float wallCatchHeight;
	[Tooltip("Anything having a normal with an absolute y value at or below this variable is considered a wall")]
	public float maxWallYNormal;
	public Vector3 wallJumpAngle;

	[Header("Fall Damage")]
	public float maxHealth;
	public float healthRecoverDelay, healthPerSecond;
	[Tooltip("Min/max distance fallen at which fall damage can occur. Damage is lerped between 0 and maxHealth where minDistance = (0) damage & maxDistance = (maxHealth) damage.")]
	public float minDamageDistance, maxDamageDistance;

	[Header("Collisions")]
	[Tooltip("How far above/below y0 the ground-check spherecast starts")]
	public float groundCheckYOffset;
	[Tooltip("How far the ground-check spherecast travels")]
	public float groundCheckDistance;

	//Private
	int wallDirection;
	float mass, _tilt, currentDrag, health;
	bool queueJump, /*queueDash,*/ canDoubleJump, _isGrounded, _isWallrunning, _isSliding, queueRoll;
	Vector3 moveDirection, currentGroundPosition;
	RaycastHit groundHit, wallHit;
	Vector xzVelocity, yVelocity;
	PlayerCamera playerCamera;
	Player player;
	Coroutine /*crtDash,*/ crtTilt, crtSlide, crtQueueRoll, crtHealth;
	HumanoidAnimatorManager animator;
	Console.Line cnsDebug;
	TMP_Text txtWhereAmI;
	VignetteControl healthVignette;
	new Camera camera;
	new CapsuleCollider collider;
	[HideInInspector] public new Rigidbody rigidbody;
	[HideInInspector] public LeapObject closestLeapObject;
	[HideInInspector] public CatchLedge closestCatchLedge;//not used rn
	#endregion
	#region Properties
	public float CurrentTilt { get => _tilt; private set { _tilt = value; playerCamera.rotationOffset = new Vector3(0, 0, _tilt); } }
	public bool IsGrounded { get => _isGrounded; private set { _isGrounded = value; animator.grounded = value; } }
	public bool IsWallrunning { get => _isWallrunning; private set { _isWallrunning = value; animator.wallRunning = value; } }
	public bool IsSliding { get => _isSliding; private set { _isSliding = value; animator.sliding = value; } }
	public bool IsOnWall { get => wallDirection != 0; }
	#endregion
	#region Unity
	IEnumerator Start()
	{
		xzVelocity = new Vector(this);
		yVelocity = new Vector(this);
		health = maxHealth;
		player = GetComponent<Player>();
		collider = transform.Find("Body").GetComponent<CapsuleCollider>();
		animator = collider.transform.Find("Model").GetComponent<HumanoidAnimatorManager>();
		Transform ui = transform.Find("UI");
		txtWhereAmI = ui.Find("WhereAmI").GetComponent<TMP_Text>();
		txtWhereAmI.text = "";
		healthVignette = ui.Find("Health Vignette").GetComponent<VignetteControl>();
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.isKinematic = false;
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
			yield return null;
			yield return new WaitForFixedUpdate();//bugs out for some reason if gravity is enabled on first frame
			useGravity = true;
		}
	}

	void Update()
	{
		if (Input.GetButtonDown("Jump")) queueJump = true;
		if (Input.GetButtonDown("Crouch") && !IsGrounded && !IsSliding) QueueRoll();
		//if (Input.GetButtonDown("Dash")) queueDash = true;
	}

	void FixedUpdate()
	{
		//Input
		moveDirection = (collider.transform.forward * Input.GetAxis("Vertical") + collider.transform.right * Input.GetAxis("Horizontal")).normalized;

		//Control
		CheckGround();
		CheckWalls();
		//CatchWallLedge();
		MovePlayer();
		Jump();
		//Dash();
		Slide();
		ControlRigidbody();
		ControlFOV();
		ControlText();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!IsGrounded)
		{
			Vector3 lastGroundPos = currentGroundPosition;
			CheckGround();//this will call twice a frame but cant be bothered
			if (IsGrounded)
			{
				float distance = lastGroundPos.y - collider.transform.position.y;
				if (queueRoll && distance >= fallRollEngageDistance)//if queueing a roll & hit the ground at roll distance
				{
					animator.roll = true;
					queueRoll = false;
					StopCoroutine(crtQueueRoll);
					crtQueueRoll = null;
					xzVelocity.vector = Vector3.ClampMagnitude(xzVelocity.vector + (-yVelocity.y * moveDirection), walkForce.maxForce);//add y velocity to forward velocity in direction of movement, dont go faster than maxForce
				}
				else if (distance >= fallCrouchEngageDistance)//if hit the ground at crouch distance
				{
					animator.land = true;
					if (crtQueueRoll != null)//if a queued roll is currently waiting for requeueTime, re-enable rolls
					{
						queueRoll = false;
						StopCoroutine(crtQueueRoll);
						crtQueueRoll = null;
					}
					float damageMagnitude = Mathf.InverseLerp(minDamageDistance, maxDamageDistance, distance);
					if (damageMagnitude > 0)
					{
						xzVelocity.vector *= 1 - damageMagnitude;//reduce lateral velocity according to percent of health decreased
						Health(Mathf.Lerp(0, maxHealth, damageMagnitude), DeathType.Fall);
					}
				}
				yVelocity.vector.y = 0;//set y velocity to 0 if we ground this frame
			}
		}
		xzVelocity.vector = rigidbody.velocity;//set lateral velocity to rigidbody velocity; bodge-job way to account for collisions
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		if (collider == null) collider = transform.Find("Body").GetComponent<CapsuleCollider>();
		Gizmos.DrawWireSphere(collider.transform.position + new Vector3(0, groundCheckYOffset + collider.radius), collider.radius);
	}
	#endregion
	#region Movement
	void MovePlayer()
	{
		if (IsGrounded)//on ground
		{
			if (IsWallrunning) WallRun(false);

			if (moveDirection == Vector3.zero) currentDrag = noInputGroundDrag.Evaluate(xzVelocity.magnitude);//not pressing any input
			else
			{
				if (IsSliding)//crouching/sliding
				{
					currentDrag = slideDrag.Evaluate(xzVelocity.magnitude);
					Vector3 force = Vector3.ProjectOnPlane(moveDirection, groundHit.normal).normalized;
					if (force.y < 0.001f) currentDrag *= groundHit.normal.y * slopeDragMultiplier;//if crouching down a slope, slide
					xzVelocity.AddForce(walkForce, force, ForceMode.Acceleration);
				}
				else//walking/running normally
				{
					currentDrag = walkDrag;
					xzVelocity.AddForce(walkForce, Vector3.ProjectOnPlane(moveDirection, groundHit.normal).normalized, ForceMode.Acceleration);
				}
			}
		}
		else if (IsOnWall && (!Physics.Raycast(collider.transform.position + (collider.transform.up * (collider.height / 2)), Vector3.down, wallCatchHeight + (collider.height / 2))))//if touching a wall and the player has jumped high enough
		{
			if (!IsWallrunning) WallRun(true);
			currentDrag = wallDrag;
			if (moveDirection != Vector3.zero) xzVelocity.AddForce(wallForce, Input.GetAxis("Vertical") * Vector3.ProjectOnPlane(collider.transform.forward, wallHit.normal).normalized, ForceMode.Acceleration);
			xzVelocity.AddForce(new Vector3(0, -wallRunGravity), ForceMode.Acceleration);
		}
		else//in air
		{
			if (IsWallrunning) WallRun(false);
			currentDrag = airDrag;
			if (moveDirection != Vector3.zero) xzVelocity.AddForce(airForce, moveDirection, ForceMode.Acceleration);
		}
	}

	/// <summary>
	/// Convert all input & calculations from this frame into rigidbody velocity
	/// </summary>
	void ControlRigidbody()
	{
		if (useGravity && !IsGrounded) yVelocity.AddForce(gravity, ForceMode.Acceleration);//gravity
		Vector3 velocity = xzVelocity.Drag(currentDrag) + yVelocity.Drag(currentDrag);//return sum of x,y and z velocities after subtracting drag
		velocity *= Time.timeScale;
		rigidbody.velocity = velocity;
		animator.velocity = velocity;
		PrintForce(velocity);
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

	void CheckGround()
	{
		if (Physics.SphereCast(collider.transform.position + (collider.transform.up * (groundCheckYOffset + collider.radius)), collider.radius, -collider.transform.up, out groundHit, groundCheckDistance, ~(1 << 3)))
		{
			if (!IsGrounded) IsGrounded = true;
			currentGroundPosition = collider.transform.position;
			return;
		}
		IsGrounded = false;
	}

	void CheckWalls()
	{
		if (!Raycast(collider.transform.right, 1) && !Raycast(-collider.transform.right, -1))
			wallDirection = 0;

		bool Raycast(Vector3 vector, int direction)
		{
			if (Physics.Raycast(collider.transform.position, vector, out wallHit, wallCatchDistance))
			{
				if (Mathf.Abs(wallHit.normal.y) <= maxWallYNormal)
				{
					wallDirection = direction;
					currentGroundPosition = collider.transform.position;//called after CheckGround(), using wall position as ground position for fall-damage checks
					return true;
				}
			}
			return false;
		}
	}

	void WallRun(bool enable)
	{
		IsWallrunning = enable;
		useGravity = !enable;
		if (enable)
		{
			canDoubleJump = true;
			if (yVelocity.y > maxWallUpwardsVelocity)
				yVelocity.vector.y = maxWallUpwardsVelocity;//prevent player from going over wall when hitting it
		}
		ResetRoutine(TweenFloat(() => CurrentTilt, (float tilt) => CurrentTilt = tilt, enable ? wallTilt * wallDirection : 0, tiltPerSecond), ref crtTilt);
		//ResetRoutine(LerpFloat(() => camera.fieldOfView, (float fov) => camera.fieldOfView = fov, fov, fovPerSecond), ref crtFOV); --not using wallrun fov rn
	}

	//void CatchWallLedge()
	//{
	//	if (!OnLedge && !IsGrounded && closestCatchLedge != null && closestCatchLedge != lastLedge)
	//	{
	//		OnLedge = true;
	//		velocity = Vector3.zero;
	//		lastLedge = closestCatchLedge;
	//		allowMovement = false;
	//		rigidbody.isKinematic = true;
	//		StartCoroutine(LerpToPos(transform, new Position(transform.position.x, lastLedge.bodyPos.position.y, transform.position.z), 5, () => allowMovement = !(rigidbody.isKinematic = false)));
	//	}
	//}

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
				if (IsWallrunning)
				{
					Force((collider.transform.up + wallHit.normal + wallJumpAngle).normalized * wallJumpForce);
				}
				else if (IsGrounded)
				{
					Force(collider.transform.up * jumpForce);
					canDoubleJump = false;
				}
				else if (canDoubleJump)
				{
					Force(collider.transform.up * doubleJumpForce);
					canDoubleJump = false;
				}
			}
		}

		void Force(Vector3 force)
		{
			yVelocity.vector.y = 0f;
			yVelocity.AddForce(force, ForceMode.VelocityChange);
		}
	}

	//void Dash()
	//{
	//	if (queueDash)
	//	{
	//		queueDash = false;
	//		if (crtDash == null && IsGrounded) crtDash = StartCoroutine(Routine());
	//	}

	//	IEnumerator Routine()
	//	{
	//		xzVelocity.AddForce(moveDirection * dashForce, ForceMode.VelocityChange);
	//		yield return new WaitForSeconds(dashCooldown);
	//		crtDash = null;
	//	}
	//}
	#endregion
	#region Misc
	public void ResetVelocity()
	{
		xzVelocity.vector = Vector3.zero;
		yVelocity.vector = Vector3.zero;
		rigidbody.velocity = Vector3.zero;
	}

	public void EnableCollider(bool enable)
	{
		collider.enabled = enable;
	}

	void PrintForce(Vector3 intendedVelocity)
	{
		if (Console.Enabled)
		{
			cnsDebug.text = $"X & Z force: {xzVelocity.magnitude:#.00} {xzVelocity} ({xzVelocity.curvePercentDebug} of current curve)\n" +
				$"Y force: {yVelocity.magnitude:#.00} {yVelocity}\n" +
				$"Intended velocity: {intendedVelocity.magnitude:#.00} {intendedVelocity}, actual rigidbody velocity: {rigidbody.velocity.magnitude:#.00} {rigidbody.velocity}\n" +
				$"Drag: {currentDrag:#00.00}\n" +
				$"Grounded: {IsGrounded}, last object walked on: {(groundHit.transform != null ? groundHit.transform.name : "not found")}\n" +
				$"Wallrunning: {IsWallrunning}, in air: {!IsGrounded && !IsWallrunning}, on wall: {(IsOnWall ? $"True, direction: {wallDirection})" : "False")}\n" +
				$"Can doublejump: {canDoubleJump}\n" +
				$"Health: {health:#0.00}, last ground/wall positon: {currentGroundPosition}";
		}
	}

	void ControlFOV()
	{
		camera.fieldOfView = TweenFloat(camera.fieldOfView, fovCurve.Evaluate(xzVelocity.magnitude), fovPerSecond);
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

	void ControlText()
	{
		if (wallHit.transform != null) txtWhereAmI.text = wallHit.transform.name;
		else if (IsGrounded && groundHit.transform != null) txtWhereAmI.text = groundHit.transform.name;
	}
	#endregion
	#region Classes
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

	class Vector
	{
		public Vector3 vector;
		ForceCurve currentCurveDebug;
		readonly PlayerMovement player;

		public Vector(PlayerMovement player)
		{
			this.player = player;
		}

		public float magnitude { get => vector.magnitude; }
		public float x { get => vector.x; }
		public float y { get => vector.y; }
		public float z { get => vector.z; }

		public string curvePercentDebug
		{
			get => currentCurveDebug == null ? "" : $"{Mathf.InverseLerp(currentCurveDebug.minForce, currentCurveDebug.maxForce, currentCurveDebug.Evaluate(magnitude)) * 100:#0}%";
		}

		public void AddForce(ForceCurve forceCurve, Vector3 force, ForceMode forceMode)
		{
			currentCurveDebug = forceCurve;
			AddForce(forceCurve.Evaluate(magnitude, force), forceMode);
		}

		public void AddForce(Vector3 force, ForceMode forceMode)
		{
			switch (forceMode)
			{
				case ForceMode.Force:
					vector += force / player.mass * Time.fixedDeltaTime;
					break;
				case ForceMode.Acceleration:
					vector += force * Time.fixedDeltaTime;
					break;
				case ForceMode.Impulse:
					vector += force / player.mass;
					break;
				case ForceMode.VelocityChange:
					vector += force;
					break;
			}
		}

		public Vector3 Drag(float drag)
		{
			return vector *= 1 - Time.fixedDeltaTime * drag;
		}

		public override string ToString()
		{
			return vector.ToString();
		}
	}
	#endregion
}

/*
 * if (rigidbody.velocity.y > yVelocity.y)//rigidbody y velocity will differ from intended velocity if a ground collision occurred, but will not always equal 0 due to interpolation (i think)
		{
			CheckGround();//this will call twice a frame but cant be bothered
			if (IsGrounded)
			{
				if (queueRoll && yVelocity.y <= -fallRollEngageSpeed)//if queueing a roll & hit the ground at roll speed
				{
					animator.roll = true;
					queueRoll = false;
					StopCoroutine(crtQueueRoll);
					crtQueueRoll = null;

					lastFallVelocity = -yVelocity.y;//store y value as positive
					yVelocity.vector.y = 0;//set y velocity to 0
					xzVelocity.vector = Vector3.ClampMagnitude(xzVelocity.vector + (lastFallVelocity * moveDirection), walkForce.maxForce);//add y velocity to forward velocity in direction of movement, dont go faster than maxForce
					return;
				}
				else if (yVelocity.y <= -fallCrouchEngageSpeed)//if hit the ground at crouch speed
				{
					animator.land = true;
					if (crtQueueRoll != null)//if a queued roll is currently waiting for requeueTime, re-enable rolls
					{
						queueRoll = false;
						StopCoroutine(crtQueueRoll);
						crtQueueRoll = null;
					}

					lastFallVelocity = -yVelocity.y;
					yVelocity.vector.y = 0;//set y velocity to 0
					float velocityPercent = Mathf.InverseLerp(minDamageVelocity, maxDamageVelocity, lastFallVelocity);
					xzVelocity.vector *= 1 - velocityPercent;//reduce lateral velocity according to percent of health decreased
					Health(Mathf.Lerp(0, maxHealth, velocityPercent), DeathType.Fall);
					return;
				}
			}
		}
		//if collision wasn't a ground collision
		xzVelocity.vector = rigidbody.velocity;//account for collisions
 */