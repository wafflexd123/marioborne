using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPlus
{
	#region Variables
	public MovementValues wall, slide, ground, air;

	[field: Header("Wall")]
	[field: SerializeField] public float wallTilt { get; set; }
	[field: SerializeField] public float tiltPerSecond { get; set; }
	[field: SerializeField, Tooltip("Y velocity is clamped to this value when entering a wall run; helps prevent player from going over walls when hitting them")] public float maxWallUpwardsVelocity { get; set; }
	[field: SerializeField, Tooltip("How high you have to be before grabbing a wall (means you have to jump onto a wall to wallrun/walljump, instead of just walk into it)")] public float wallCatchHeight { get; set; }
	[field: SerializeField, Tooltip("How far the wall check raycast travels beyond the collider")] public float wallCatchDistance { get; set; }
	[field: SerializeField, Tooltip("Anything having a normal with an absolute y value at or below this variable is considered a wall")] public float maxWallYNormal { get; set; }

	[field: Header("Slide")]
	[field: SerializeField, Tooltip("Multiplier for the amount a slope's steepness affects sliding acceleration")] public float slopeDragMultiplier { get; set; }

	[field: Header("Air")]
	[field: SerializeField] public int maxAirJumps { get; set; }
	[field: SerializeField] public int maxAirDashes { get; set; }
	[field: SerializeField] public float dashForce { get; set; }
	[field: SerializeField, Tooltip("How many seconds before you hit the ground that a roll can be registered")] public float rollQueueTime { get; set; }
	[field: SerializeField, Tooltip("How many seconds, after rollQueueTime, before allowing a roll again (stops player from pressing the roll button too early)")] public float rollRequeueTime { get; set; }
	[field: SerializeField, Tooltip("Minimum distance fallen at which rolls can occur")] public float fallRollEngageDistance { get; set; }
	[field: SerializeField, Tooltip("Minimum distance fallen at which the fall-crouch animation can occur")] public float fallCrouchEngageDistance { get; set; }

	[field: Header("Fall Damage")]
	[field: SerializeField] public float maxHealth { get; set; }
	[field: SerializeField] public float healthRecoverDelay { get; set; }
	[field: SerializeField] public float healthPerSecond { get; set; }
	[field: SerializeField, Tooltip("Min distance fallen at which fall damage can occur. Damage is lerped between 0 and maxHealth where minDistance = (0) damage & maxDistance = (maxHealth) damage.")] public float minDamageDistance { get; set; }
	[field: SerializeField, Tooltip("Max distance fallen at which fall damage can occur. Damage is lerped between 0 and maxHealth where minDistance = (0) damage & maxDistance = (maxHealth) damage.")] public float maxDamageDistance { get; set; }

	[Header("Misc")]
	[Tooltip("Curve where x=magnitude of velocity and y=magnitude of FOV")] public ForceCurve fovCurve;
	[field: SerializeField, Tooltip("Max change in FOV per second")] public float fovPerSecond { get; set; }
	[field: SerializeField, Tooltip("Do not use rigidbody.gravity, use this!")] public bool useGravity { get; set; }

	//Non-inspector public properties
	public float currentTilt { get => _tilt; private set { _tilt = value; playerCamera.rotationOffset = new Vector3(0, 0, _tilt); } }
	public bool isGrounded { get => _isGrounded; private set { _isGrounded = value; animator.grounded = value; } }
	public bool isSliding { get; private set; }
	public State currentState { get; private set; }
	public bool enableInput { get; set; }
	public Vector3 lookDirection => camera.transform.forward;
	public bool isOnWall => wallDirection != 0;

	//Private
	int wallDirection, airJumpCount, airDashCount;
	float mass, _tilt, currentDrag, health;
	readonly float groundCheckYOffset = .01f, groundCheckDistance = .001f;
	bool queueJump, _isGrounded, queueRoll, queueDash;
	Vector3 moveDirection, currentGroundPosition, lastActualVelocity;
	RaycastHit groundHit, wallHit;
	Vector xzVelocity, yVelocity;
	PlayerCamera playerCamera;
	Player player;
	Coroutine crtTilt, crtQueueRoll, crtHealth, crtDash;
	HumanoidAnimatorManager animator;
	Console.Line cnsDebug;
	TMP_Text txtWhereAmI;
	VignetteControl healthVignette;
	new Camera camera;
	new CapsuleCollider collider;
	readonly State[] movementStates = new State[4];
	[HideInInspector] public new Rigidbody rigidbody;
	[HideInInspector] public LeapObject closestLeapObject;

	#endregion
	#region Unity
	IEnumerator Start()
	{
		//Init methods
		StartCoroutine(AdjustVelocityForCollisions());
		InitialiseStates();

		//Variable init
		xzVelocity = new Vector(this);
		yVelocity = new Vector(this);
		health = maxHealth;
		enableInput = true;
		cnsDebug = Console.AddLine();

		//GetComponents
		player = GetComponent<Player>();
		collider = transform.Find("Body").GetComponent<CapsuleCollider>();
		animator = collider.transform.Find("Model").GetComponent<HumanoidAnimatorManager>();
		playerCamera = transform.Find("Head").GetComponent<PlayerCamera>();
		camera = playerCamera.transform.Find("Eyes").Find("Camera").GetComponent<Camera>();

		//UI
		Transform ui = transform.Find("UI");
		txtWhereAmI = ui.Find("WhereAmI").GetComponent<TMP_Text>();
		txtWhereAmI.text = "";
		healthVignette = ui.Find("Health Vignette").GetComponent<VignetteControl>();

		//Rigidbody
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

		//Wait a frame before applying force; otherwise, player is occaisonally shot into the air in the first frame
		bool temp = useGravity;
		useGravity = false;
		currentGroundPosition = transform.position;
		yield return null;
		useGravity = temp;
		ResetVelocity();
	}

	void Update()
	{
		if (enableInput)
		{
			if (Input.GetButtonDown("Jump")) queueJump = true;
			if (Input.GetButtonDown("Crouch") && !isGrounded && !isSliding) QueueRoll();
			if (Input.GetButtonDown("Dash")) queueDash = true;
		}
	}

	void FixedUpdate()
	{
		//Input
		moveDirection = enableInput ? (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")).normalized : Vector3.zero;

		//Control
		CheckGround();
		CheckWalls();
		MovementState();
		ControlRigidbody();
		ControlFOV();
		ControlText();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!isGrounded)
		{
			Vector3 lastGroundPos = currentGroundPosition;
			if ((rigidbody.velocity.y / Time.timeScale) - xzVelocity.y - yVelocity.y > .01f)//if there is a difference between the actual rigidbody y velocity and the applied y velocity; probably not 100% reliable when jumping off non-level ground, should fix
			{
				float distance = lastGroundPos.y - transform.position.y;
				if (queueRoll && distance >= fallRollEngageDistance)//if queueing a roll & hit the ground at roll distance
				{
					animator.roll = true;
					queueRoll = false;
					StopCoroutine(crtQueueRoll);
					crtQueueRoll = null;
					xzVelocity.vector = Vector3.ClampMagnitude(xzVelocity.vector + (-yVelocity.y * moveDirection), ground.YforceXvelocity.maxY);//add y velocity to forward velocity in direction of movement, dont go faster than maxForce
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
				rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);//set velocity instantly to 0 instead of lerping over a few frames. doesn't work. nice.
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		if (collider == null) collider = transform.Find("Body").GetComponent<CapsuleCollider>();
		Gizmos.DrawWireSphere(transform.position + new Vector3(0, -groundCheckDistance + collider.radius), collider.radius);
	}

	#endregion
	#region Movement
	void MovementState()
	{
		foreach (State state in movementStates)
		{
			if (state == currentState)
			{
				if (!currentState.wantsExit()) break;//unless state wants to force exit, only previous states in array can override and enter
			}
			else if (state.canEnter())
			{
				currentState.exitState();
				state.enterState();
				currentState = state;
				break;
			}
		}

		currentState.move();
		if (queueJump)
		{
			currentState.jump();
			queueJump = false;
		}
	}

	void InitialiseStates()
	{
		//----WALL STATE----
		movementStates[0] = new State("Wall", wall,
			move: () =>
			{
				State.DefaultMove(this, wall, Input.GetAxis("Vertical") * Vector3.ProjectOnPlane(transform.forward, wallHit.normal).normalized);
			},
			jump: () =>
			{
				State.DefaultJump(this, wall, wallHit.normal);
			},
			enterState: () =>
			{
				animator.wallRunning = true;
				if (yVelocity.y > maxWallUpwardsVelocity) yVelocity.vector.y = maxWallUpwardsVelocity;//prevent player from going over wall when hitting it
				ResetRoutine(TweenFloat(() => currentTilt, (float tilt) => currentTilt = tilt, wallTilt * wallDirection, tiltPerSecond), ref crtTilt);
			},
			exitState: () =>
			{
				ResetRoutine(TweenFloat(() => currentTilt, (float tilt) => currentTilt = tilt, 0, tiltPerSecond), ref crtTilt);
			},
			canEnter: () =>
			{
				return isOnWall && (!Physics.Raycast(transform.position + (transform.up * (collider.height / 2)), Vector3.down, wallCatchHeight + (collider.height / 2)));//if touching wall && far enough from ground
			},
			wantsExit: () =>
			{
				return !isOnWall || Physics.Raycast(transform.position + (transform.up * (collider.height / 2)), Vector3.down, wallCatchHeight + (collider.height / 2));//if not touching a wall || too close to the ground
			});

		//----SLIDE STATE----
		movementStates[1] = new State("Slide", slide,
			move: () =>
			{
				Vector3 force = Vector3.ProjectOnPlane(moveDirection, groundHit.normal).normalized;
				if (force.y <= -0.001f) currentDrag *= groundHit.normal.y * slopeDragMultiplier;//if crouching down (down is <= -0.001f) a slope, slide
				State.DefaultMove(this, slide, force);
			},
			jump: () =>
			{
				State.DefaultJump(this, slide, Vector3.zero);
			},
			enterState: () =>
			{
				isSliding = true;
				animator.sliding = true;
			},
			exitState: () =>
			{
				isSliding = false;
				animator.sliding = false;
			},
			canEnter: () =>
			{
				return Input.GetButton("Crouch") && isGrounded;
			},
			wantsExit: () =>
			{
				return !Input.GetButton("Crouch") || !isGrounded;
			});

		//----GROUND STATE----
		movementStates[2] = new State("Ground", ground,
			move: () =>
			{
				State.DefaultMove(this, ground, Vector3.ProjectOnPlane(moveDirection, groundHit.normal).normalized);
			},
			jump: () =>
			{
				State.DefaultJump(this, ground, Vector3.zero);
			},
			enterState: () =>
			{
			},
			exitState: () =>
			{
			},
			canEnter: () =>
			{
				return isGrounded;
			},
			wantsExit: () =>
			{
				return !isGrounded;
			});

		//----AIR STATE----
		currentState = movementStates[3] = new State("Air", air,
			move: () =>
			{
				State.DefaultMove(this, air, moveDirection);
				if (queueDash)
				{
					queueDash = false;
					if (airDashCount < maxAirDashes)
					{
						yVelocity.AddForce(lookDirection * dashForce, ForceMode.VelocityChange);
						airDashCount++;
					}
				}
			},
			jump: () =>
			{
				if (airJumpCount < maxAirJumps)
				{
					State.DefaultJump(this, air, Vector3.zero);
					airJumpCount++;
				}
			},
			enterState: () =>
			{
			},
			exitState: () =>
			{
				airJumpCount = 0;
				airDashCount = 0;
				queueDash = false;
			},
			canEnter: () =>
			{
				return true;//last in array; if nothing else can enter, surely we are in the air (i hope)
			},
			wantsExit: () =>
			{
				return false;//other states will enter before this is checked, no need to add anything here
			});
	}

	/// <summary>
	/// Convert all input & calculations from this frame into rigidbody velocity
	/// </summary>
	void ControlRigidbody()
	{
		Vector3 velocity = xzVelocity.Drag(currentDrag) + yVelocity.Drag(currentDrag);//return sum of x,y and z velocities after subtracting drag
		velocity *= Time.timeScale;
		rigidbody.velocity = velocity;
		animator.velocity = velocity;
		PrintForce(velocity);
	}

	/// <summary>
	/// Will let a roll initiate when you hit the ground if the button was pressed during queueTime, will not let a roll initiate afterwards during requeueTime or until the frame after you hit the ground
	/// </summary>
	void QueueRoll()
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

	void CheckGround()
	{
		if (Physics.SphereCast(transform.position + (transform.up * (groundCheckYOffset + collider.radius)), collider.radius, -transform.up, out groundHit, groundCheckDistance + groundCheckYOffset, ~(1 << 3)))
		{
			if (!isGrounded) isGrounded = true;
			currentGroundPosition = transform.position;
			return;
		}
		isGrounded = false;
	}

	void CheckWalls()
	{
		if (!Raycast(transform.right, 1) && !Raycast(-transform.right, -1))
			wallDirection = 0;

		bool Raycast(Vector3 vector, int direction)
		{
			if (Physics.Raycast(transform.position, vector, out wallHit, wallCatchDistance + collider.radius))
			{
				if (Mathf.Abs(wallHit.normal.y) <= maxWallYNormal)
				{
					wallDirection = direction;
					currentGroundPosition = transform.position;//called after CheckGround(), using wall position as ground position for fall-damage checks
					return true;
				}
			}
			return false;
		}
	}

	IEnumerator AdjustVelocityForCollisions()
	{
		while (true)
		{
			yield return new WaitForFixedUpdate();
			//Vector3 rigidbodyVelocity = rigidbody.velocity / Time.timeScale;
			//rigidbodyVelocity.y -= yVelocity.y;
			//if (Mathf.Abs(rigidbodyVelocity.y) < 0.1f) rigidbodyVelocity.y = 0;//floating point precision workaround
			//if (Mathf.Abs(rigidbodyVelocity.x) < Mathf.Abs(xzVelocity.x)) xzVelocity.x = rigidbodyVelocity.x;
			//if (Mathf.Abs(rigidbodyVelocity.y) < Mathf.Abs(xzVelocity.y)) xzVelocity.y = rigidbodyVelocity.y;
			//if (Mathf.Abs(rigidbodyVelocity.z) < Mathf.Abs(xzVelocity.z)) xzVelocity.z = rigidbodyVelocity.z;

			lastActualVelocity = rigidbody.velocity;//for debug screen
		}
	}

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
			cnsDebug.text =
				$"X & Z force: {xzVelocity.magnitude:#.00} {xzVelocity} ({xzVelocity.curvePercentDebug} of force curve)\n" +
				$"Y force: {yVelocity.magnitude:#.00} {yVelocity}\n" +
				$"Intended velocity: {intendedVelocity.magnitude:#.00} {intendedVelocity}\n" +
				$"Actual velocity: {lastActualVelocity.magnitude:#.00} {lastActualVelocity}\n" +
				$"Drag: {currentDrag:#00.00}, state: {currentState.name}, last ground/wall: {currentGroundPosition}\n" +
				$"Health: {health:#0.00}";
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
		else if (isGrounded && groundHit.transform != null) txtWhereAmI.text = groundHit.transform.name;
	}
	#endregion
	#region Classes
	[System.Serializable]
	public class ForceCurve
	{
		public AnimationCurve curve;
		[Tooltip("eg: minimum force or drag")] public float minY;
		[Tooltip("eg: maximum force or drag")] public float maxY;
		[Tooltip("eg: velocity at which force or drag is maximum")] public float maxX;

		public Vector3 Evaluate(float currentX, Vector3 direction)
		{
			return Mathf.Lerp(minY, maxY, curve.Evaluate(Mathf.Clamp01(currentX / maxX))) * direction;
		}

		public float Evaluate(float currentX)
		{
			return Mathf.Lerp(minY, maxY, curve.Evaluate(Mathf.Clamp01(currentX / maxX)));
		}

		public void Multiply(float multiplier)
		{
			minY *= multiplier;
			maxY *= multiplier;
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
		public float x { get => vector.x; set => vector.x = value; }
		public float y { get => vector.y; set => vector.y = value; }
		public float z { get => vector.z; set => vector.z = value; }
		public float this[int i] { get => vector[i]; set => vector[i] = value; }

		public string curvePercentDebug
		{
			get => currentCurveDebug == null ? "?%" : $"{Mathf.InverseLerp(currentCurveDebug.minY, currentCurveDebug.maxY, currentCurveDebug.Evaluate(magnitude)) * 100:#0}%";
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

	[System.Serializable]
	public class MovementValues
	{
		[field: SerializeField] public ForceCurve YforceXvelocity { get; set; }
		[field: SerializeField] public ForceCurve YdragXvelocity { get; set; }
		[field: SerializeField] public ForceCurve noInputYdragXvelocity { get; set; }
		[field: SerializeField] public float jumpForce { get; set; }
		[field: SerializeField] public Vector3 gravity { get; set; }
	}

	public class State
	{
		public string name;
		public System.Action move, jump, enterState, exitState;
		public System.Func<bool> canEnter, wantsExit;
		public MovementValues values;

		public State(string name, MovementValues stateInspector, System.Action move, System.Action jump, System.Action enterState, System.Action exitState, System.Func<bool> canEnter, System.Func<bool> wantsExit)
		{
			this.name = name;
			this.values = stateInspector;
			this.move = move;
			this.jump = jump;
			this.enterState = enterState;
			this.exitState = exitState;
			this.canEnter = canEnter;
			this.wantsExit = wantsExit;
		}

		public static void DefaultMove(PlayerMovement p, MovementValues s, Vector3 forceDirection)
		{
			if (p.moveDirection != Vector3.zero)
			{
				p.xzVelocity.AddForce(s.YforceXvelocity, forceDirection, ForceMode.Acceleration);
				p.currentDrag = s.YdragXvelocity.Evaluate(p.xzVelocity.magnitude);
			}
			else
			{
				p.currentDrag = s.noInputYdragXvelocity.Evaluate(p.xzVelocity.magnitude);
			}
			if (p.useGravity) p.yVelocity.AddForce(s.gravity, ForceMode.Acceleration);
		}

		public static void DefaultJump(PlayerMovement p, MovementValues s, Vector3 angle)
		{
			if (p.enableInput)
			{
				p.yVelocity.vector.y = 0f;
				p.yVelocity.AddForce((p.transform.up + angle).normalized * s.jumpForce, ForceMode.VelocityChange);
			}
		}
	}
	#endregion
}
