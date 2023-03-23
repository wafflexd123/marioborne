using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	//Inspector
	public Transform head;
	public float speed, cameraSensitivity;

	//Script
	Vector3 cameraVector;
	new Rigidbody rigidbody;
	Console.Line cnsVelocity;

	private void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		cnsVelocity = Console.AddLine();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Q)) Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;
		if (Input.GetKeyDown(KeyCode.Tilde)) Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;

		float y = -Input.GetAxis("Mouse Y"), x = Input.GetAxis("Mouse X");
		cameraVector = new Vector3(Mathf.Clamp((y * Time.deltaTime * cameraSensitivity) + cameraVector.x, -45, 45), ((x * Time.deltaTime * cameraSensitivity) + cameraVector.y) % 360f);
		transform.localEulerAngles = new Vector3(0, cameraVector.y);
		head.transform.localEulerAngles = new Vector3(cameraVector.x, 0);
	}

	private void FixedUpdate()
	{
		float hor = Input.GetAxis("Horizontal"), ver = Input.GetAxis("Vertical");
		if (hor != 0 || ver != 0)
			rigidbody.velocity = transform.TransformDirection(speed * Time.fixedDeltaTime * Vector3.ClampMagnitude(new Vector3(hor, 0, ver), 1));
		else if (rigidbody.velocity != Vector3.zero)
			rigidbody.velocity = Vector3.zero;
		if (Console.Enabled) cnsVelocity.text = $"Player velocity: {rigidbody.velocity}; Horizontal: {Mathf.Sqrt((rigidbody.velocity.x * rigidbody.velocity.x) + (rigidbody.velocity.z * rigidbody.velocity.z)):#.00}";
	}
}
