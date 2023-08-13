using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCamera : MonoBehaviour
{
	public float sensitivity, minAngle, maxAngle;
	[HideInInspector] public Vector3 rotationOffset;
	Vector3 rotation;

	//for the UI slider
	public float Sensitivity { set { sensitivity = value; GameManager.singleton.savedSensitivity = value; } get => sensitivity; }

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	void OnEnable()
	{
		rotation = new Vector3(transform.localEulerAngles.x, transform.parent.localEulerAngles.y);
	}

	void Update()
	{
		rotation.x = Mathf.Clamp(rotation.x - (Input.GetAxis("Mouse Y") * sensitivity), minAngle, maxAngle);
		rotation.y += Input.GetAxisRaw("Mouse X") * sensitivity % 360;
		transform.localEulerAngles = new Vector3(rotation.x, 0) + rotationOffset;
		transform.parent.localEulerAngles = new Vector3(0, rotation.y);

		if (Input.GetKeyDown(KeyCode.Tab)) Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
	}
}
