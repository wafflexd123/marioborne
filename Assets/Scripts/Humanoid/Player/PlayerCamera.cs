using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCamera : MonoBehaviour
{
	public float sensitivity;
	public Transform body;
	[HideInInspector] public Vector3 rotationOffset;
	Vector3 rotation;

	//for the UI slider
	public float Sensitivity { set => sensitivity = value; }

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void Update()
	{
		rotation.x = Mathf.Clamp(rotation.x - (Input.GetAxis("Mouse Y") * sensitivity), -90, 90);
		rotation.y += Input.GetAxisRaw("Mouse X") * sensitivity % 360;
		transform.eulerAngles = rotation + rotationOffset;
		body.localEulerAngles = new Vector3(0, rotation.y);
	}
}
