using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField] Transform orientation;
	[Header("Movement")]
	public float walkSpeed = 6f;
	public float walkMultiplier = 10f, airMultiplier = 0.8f, dashCooldown = 2f, dashForce = 30f, jumpForce = 15f, playerHeight = 1.8f;

	[Header("Drag")]
	public float groundDrag = 6f;
	public float airDrag = 0.8f;

	[Header("Ground Detection")]
	[SerializeField] Transform groundCheck;
	[SerializeField] float groundDistance = 0.2f;
	[SerializeField] LayerMask whatIsGround;

    [Header("Wall Jump")]
    [SerializeField] float wallDistance = 0.6f;
    public float wallJumpDelay = 0.1f;
    private bool wall;

	float mass;
    bool queueJump, queueDash, canDoubleJump = true;
    public bool isGrounded, canWallJump;
	Vector3 moveDirection;
	new Rigidbody rigidbody;
	WallRunV2 wallRun;
	Coroutine crtDash;
    Coroutine crtWallJump;

	private void Start()
	{
		wallRun = GetComponent<WallRunV2>();
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.freezeRotation = true;
		mass = rigidbody.mass;
	}

	void Update()
	{
		if (Input.GetButtonDown("Jump")) queueJump = true;
		if (Input.GetButtonDown("Dash")) queueDash = true;
		rigidbody.mass = mass / Time.timeScale;
        CheckWall();
	}

	void FixedUpdate()
	{
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);
		moveDirection = (orientation.forward * Input.GetAxisRaw("Vertical") + orientation.right * Input.GetAxisRaw("Horizontal")).normalized;
		ControlDrag();
		MovePlayer();
		Jump();
		Dash();
	}

	void MovePlayer()
	{
		if (isGrounded)
		{
			if (OnSlope(out RaycastHit slopeHit)) rigidbody.AddForce(walkMultiplier * walkSpeed * Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized, ForceMode.Force);
			else rigidbody.AddForce(walkMultiplier * walkSpeed * moveDirection, ForceMode.Force);
		}
		else
		{
			rigidbody.AddForce(airMultiplier * walkSpeed * moveDirection, ForceMode.Force);
		}
	}

	void Jump()
	{
        if (isGrounded)
        {
            canDoubleJump = true;
            canWallJump = false;
            if (crtWallJump == null && !wallRun.isWallrunning) crtWallJump = StartCoroutine(Routine());
        }

		if (queueJump)
		{
			queueJump = false;
			if (isGrounded)
			{
				Force();
			}
			else if (canDoubleJump && !wallRun.isWallrunning)
			{
				Force();
				canDoubleJump = false;
			}
		}

        if (canWallJump)
        {
            if (wall)
            {
                Force();
                canWallJump = false;
            }
        }

        void Force()
		{
			rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
			rigidbody.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
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
		rigidbody.drag = isGrounded || wallRun.isWallrunning ? groundDrag : airDrag;
	}

	bool OnSlope(out RaycastHit slopeHit)
	{
		return Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f) && slopeHit.normal != Vector3.up;
	}

    void CheckWall()
    {
        wall = Physics.Raycast(transform.position + transform.up, orientation.forward, wallDistance);
    }
}
