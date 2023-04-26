using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunV2 : MonoBehaviourPlus
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

	[Header("Camera")]
	public float wallFov, fovPerSecond, wallTilt, tiltPerSecond;
	new Camera camera;
	float startFov;

	public float currentTilt { get => _tilt; private set { _tilt = value; camera.GetComponent<PlayerCamera>().rotationOffset = new Vector3(0, 0, _tilt); } }
	float _tilt;

	private bool wallLeft = false;
	private bool wallRight = false;
	public bool isWallrunning = false;

	RaycastHit leftWallHit;
	RaycastHit rightWallHit;

	new Rigidbody rigidbody;
	Coroutine crtFOV, crtTilt;

	private void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		camera = transform.Find("Eyes").Find("Camera").GetComponent<Camera>();
		startFov = camera.fieldOfView;
	}

	private void Update()
	{
		CheckWall();

		if (CanWallRun())
		{
			if (wallLeft)
			{
				WallRun();
				//StartWallRun();
			}
			else if (wallRight)
			{
				WallRun();
				//StartWallRun();
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


	void WallRun()
	{
		rigidbody.useGravity = false;
		isWallrunning = true;
		ResetRoutine(LerpFloat(() => camera.fieldOfView, (float fov) => camera.fieldOfView = fov, wallFov, fovPerSecond), ref crtFOV);
		ResetRoutine(LerpFloat(() => currentTilt, (float tilt) => currentTilt = tilt, wallLeft ? -wallTilt : wallTilt, tiltPerSecond), ref crtTilt);
		StartCoroutine(Run());
		IEnumerator Run()
		{
			while (true)
			{
				rigidbody.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);
				rigidbody.AddForce(orientation.forward * wallRunSpeed, ForceMode.Acceleration);

				yield return new WaitForFixedUpdate();
			}
		}
	}

	void StopWallRun()
	{
		isWallrunning = false;
		rigidbody.useGravity = true;

		camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, startFov, fovPerSecond * Time.deltaTime);
		currentTilt = Mathf.Lerp(currentTilt, 0, tiltPerSecond * Time.deltaTime);
		currentTilt = 0;
	}

	void StartWallRun()
	{
		rigidbody.useGravity = false;
		isWallrunning = true;

		rigidbody.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);
		rigidbody.AddForce(orientation.forward * wallRunSpeed, ForceMode.Acceleration);

		camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, wallFov, fovPerSecond * Time.deltaTime);

		if (wallLeft)
		{
			currentTilt = -wallTilt;
			//tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
		}

		else if (wallRight)
		{
			currentTilt = wallTilt;
			// tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime);
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (wallLeft)
			{
				Vector3 wallRunJumpDirection = transform.up + leftWallHit.normal;
				rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
				rigidbody.AddForce(wallRunJumpDirection * wallJumpForce * 100, ForceMode.Force);
			}
			else if (wallRight)
			{
				Vector3 wallRunJumpDirection = transform.up + rightWallHit.normal;
				rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
				rigidbody.AddForce(wallRunJumpDirection * wallJumpForce * 100, ForceMode.Force);
			}
		}
	}
}
