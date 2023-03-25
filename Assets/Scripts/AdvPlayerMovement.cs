using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintSpeed;
    public float runSpeed;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float groundDistance = 0.4f;
    public float playerHeight;
    public float groundDrag;

    public LayerMask whatIsGround;

    bool grounded;
    bool readyToJump;

    public Transform orientation;
    public Transform groundCheck;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
    }

    private void Update()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);

        GetInput();

        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

        SpeedControl();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (Input.GetKey(sprintKey))
        {
            moveSpeed = sprintSpeed;
        }
        else
        {
            moveSpeed = runSpeed;
        }

    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
