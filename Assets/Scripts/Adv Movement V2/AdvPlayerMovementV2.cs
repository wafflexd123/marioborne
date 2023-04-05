using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvPlayerMovementV2 : MonoBehaviour
{
	[Header("Movement")]
	private float moveSpeed;
	public float walkSpeed = 6f;
	public float sprintSpeed = 12f;
	public float movementMultiplier = 10f;
	public float playerHeight = 2;
	[SerializeField] float acceleration;
	[SerializeField] float airMultiplier = 0.8f;
	[SerializeField] float jumpCooldown = 0.2f;
	[SerializeField] float dashCooldown = 2f;
	[SerializeField] Transform orientation;
	[SerializeField] float jumpForce = 50f;
	[SerializeField] float dashForce = 8f;
	[SerializeField] private bool canJump = true;
	private bool canDash = true;
	[SerializeField] private bool canDoubleJump = true;

	[Header("Drag")]
	float groundDrag = 6f;
	float airDrag = 0.8f;

	float horizontalMovement;
	float verticalMovement;

	[Header("Keybinds")]
	[SerializeField] KeyCode jumpKey = KeyCode.Space;
	[SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
	[SerializeField] KeyCode dashkey = KeyCode.V;

	[Header("Ground Detection")]
	[SerializeField] Transform groundCheck;
	[SerializeField] float groundDistance = 0.2f;
	[SerializeField] LayerMask whatIsGround;
	public bool isGrounded;

	Vector3 moveDirection;
	Vector3 slopeMoveDirection;

	Rigidbody rb;
	WallRunV2 wr;
	RaycastHit slopeHit;

	private void Start()
	{
		wr = GetComponent<WallRunV2>();
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
	}

	private void Update()
	{
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);

		MyInput();
		ControlDrag();

		if (isGrounded) canDoubleJump = true;

		slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
	}

	void MyInput()
	{
		horizontalMovement = Input.GetAxisRaw("Horizontal");
		verticalMovement = Input.GetAxisRaw("Vertical");

		moveDirection = (orientation.forward * verticalMovement + orientation.right * horizontalMovement).normalized;

		if (Input.GetKey(jumpKey) && canJump && isGrounded)
		{
			canJump = false;

			Jump();

			Invoke(nameof(ResetJump), jumpCooldown);
		}

		if (Input.GetKeyDown(jumpKey) && !isGrounded && canDoubleJump && !wr.isWallrunning)
		{
			canDoubleJump = false;

			Jump();
		}

		if (Input.GetKeyDown(dashkey) && canDash && isGrounded)
		{
			canDash = false;

			Dash();

			Invoke(nameof(ResetDash), dashCooldown);
		}

		if (Input.GetKey(sprintKey) && isGrounded)
		{
			moveSpeed = sprintSpeed;
		}
		else
		{
			moveSpeed = walkSpeed;
		}
	}

	private void FixedUpdate()
	{
		MovePlayer();
	}

	void MovePlayer()
	{
		if (isGrounded)
		{
			rb.AddForce(movementMultiplier * moveSpeed * (OnSlope() ? slopeMoveDirection : moveDirection), ForceMode.Acceleration);
		}
		else
		{
			rb.AddForce(airMultiplier * moveSpeed * moveDirection, ForceMode.Acceleration);
		}
	}

	void Jump()
	{
		rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
		rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
	}

	void ResetJump()
	{
		canJump = true;
	}

	void Dash()
	{
		rb.AddForce(moveDirection * dashForce, ForceMode.Impulse);
	}

	void ResetDash()
	{
		canDash = true;
	}

	void Crouch()
	{
		// Crouch
	}

	void ControlDrag()
	{
		if (isGrounded)
		{
			rb.drag = groundDrag;
		}
		else if (wr.isWallrunning)
		{
			rb.drag = groundDrag;
		}
		else
		{
			rb.drag = airDrag;
		}
	}

	void ControlSpeed()
	{
		if (Input.GetKey(sprintKey) && isGrounded)
		{
			moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
		}
		else
		{
			moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
		}
	}

	private bool OnSlope()
	{
		if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
		{
			if (slopeHit.normal != Vector3.up)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		return false;
	}
}
