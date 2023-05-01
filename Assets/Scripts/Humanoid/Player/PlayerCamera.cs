using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerCamera : MonoBehaviour
{
	public float sensitivity, minAngle, maxAngle;
	public Transform body;
	[HideInInspector] public Vector3 rotationOffset;
	Vector3 rotation;
   



    //for the UI slider
    public float Sensitivity { set => sensitivity = value; }

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		rotation = transform.localEulerAngles;
	}

   

    void Update()
	{
		rotation.x = Mathf.Clamp(rotation.x - (Input.GetAxis("Mouse Y") * sensitivity), minAngle, maxAngle);
		rotation.y += Input.GetAxisRaw("Mouse X") * sensitivity % 360;
		transform.localEulerAngles = rotation + rotationOffset;
		body.localEulerAngles = new Vector3(0, rotation.y);

		if (Input.GetKeyDown(KeyCode.Tab)) Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
	}
}
