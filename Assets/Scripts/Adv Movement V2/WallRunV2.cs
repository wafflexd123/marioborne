using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunV2 : MonoBehaviour
{
    [SerializeField] Transform orientation;

    [Header("Detection")]
    [SerializeField] float wallDistance = 0.6f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance;
    [SerializeField] LayerMask whatIsWall;
    [SerializeField] LayerMask whatIsGround;

    [Header("Wall Running")]
    [SerializeField] private float wallRunGravity;
    [SerializeField] private float wallJumpForce;
    [SerializeField] private float wallRunSpeed;

    private bool wallLeft = false;
    private bool wallRight = false;
    public bool isWallrunning = false;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    private Rigidbody rb;
    private AdvPlayerMovementV2 playerMovement;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CheckWall();

        if (CanWallRun())
        {
            if (wallLeft)
            {
                StartWallRun();
            }
            else if (wallRight)
            {
                StartWallRun();
            }
            else
            {
                StopWallRun();
            }
        }
        else
        {
            StopWallRun();
        }
    }

    void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistance, whatIsWall);
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistance, whatIsWall);
    }

    bool CanWallRun()
    {  
        return !Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround); ;
    }

    void StartWallRun()
    {
        rb.useGravity = false;
        isWallrunning = true;

        rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);
        rb.AddForce(orientation.forward * wallRunSpeed, ForceMode.Acceleration);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (wallLeft)
            {
                Vector3 wallRunJumpDirection = transform.up + leftWallHit.normal;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallRunJumpDirection * wallJumpForce * 100, ForceMode.Force);
            }
            else if (wallRight)
            {
                Vector3 wallRunJumpDirection = transform.up + rightWallHit.normal;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallRunJumpDirection * wallJumpForce * 100, ForceMode.Force);
            }
        }
    }

    void StopWallRun()
    {
        isWallrunning = false;
        rb.useGravity = true;
    }
}
