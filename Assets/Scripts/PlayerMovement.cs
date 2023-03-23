using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	//Inspector
	public Transform head, bodyCollider;
	public float speed, cameraSensitivity, crouchHeightReduction, crouchSpeed;

	//Script
	Vector3 cameraVector;
	new Rigidbody rigidbody;
	Console.Line cnsVelocity;
	int currentCrouchDirection;
	Coroutine crtCrouch;

	void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		cnsVelocity = Console.AddLine();
	}

	void Update()
	{
		Crouch();
		Camera();
		LockCursor();
	}

	void FixedUpdate()
	{
		Movement();
	}

	void Crouch()
	{
		if (Input.GetButtonDown("Crouch") && crtCrouch == null) crtCrouch = StartCoroutine(Lerp());//crouch if not already crouching

		IEnumerator Lerp()
		{
			float totalCrouch = 0, currentCrouch = 0;
			currentCrouchDirection = currentCrouchDirection == -1 ? 1 : -1;//swap crouch direction
			while (totalCrouch != crouchHeightReduction)
			{
				currentCrouch = crouchSpeed * Time.deltaTime;
				totalCrouch += currentCrouch;
				if (totalCrouch > crouchHeightReduction)//when total crouch overshoots
				{
					currentCrouch -= totalCrouch - crouchHeightReduction;//minus how much total crouch has gone above crouchheight
					totalCrouch = crouchHeightReduction;
				}
				bodyCollider.localScale += new Vector3(0, currentCrouch * currentCrouchDirection, 0);
				head.localPosition += new Vector3(0, currentCrouch * currentCrouchDirection, 0);
				yield return null;
			}
			crtCrouch = null;
		}
	}

	void Camera()
	{
		float y = -Input.GetAxis("Mouse Y"), x = Input.GetAxis("Mouse X");
		cameraVector = new Vector3(Mathf.Clamp((y * Time.deltaTime * cameraSensitivity) + cameraVector.x, -45, 45), ((x * Time.deltaTime * cameraSensitivity) + cameraVector.y) % 360f);
		transform.localEulerAngles = new Vector3(0, cameraVector.y);
		head.transform.localEulerAngles = new Vector3(cameraVector.x, 0);
	}

	void LockCursor()
	{
		if (Input.GetKeyDown(KeyCode.Q)) Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;
	}

	void Movement()
	{
		float hor = Input.GetAxis("Horizontal"), ver = Input.GetAxis("Vertical");
		if (hor != 0 || ver != 0)
			rigidbody.velocity = transform.TransformDirection(speed * Time.fixedDeltaTime * Vector3.ClampMagnitude(new Vector3(hor, 0, ver), 1));
		else if (rigidbody.velocity != Vector3.zero)
			rigidbody.velocity = Vector3.zero;
		if (Console.Enabled) cnsVelocity.text = $"Player velocity: {rigidbody.velocity}; Horizontal: {Mathf.Sqrt((rigidbody.velocity.x * rigidbody.velocity.x) + (rigidbody.velocity.z * rigidbody.velocity.z)):#.00}";
	}
}
