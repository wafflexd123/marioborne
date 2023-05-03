using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCamera : MonoBehaviour
{
	public float sensitivity, minAngle, maxAngle;
	public Transform body;
	[HideInInspector] public Vector3 rotationOffset;
	Vector3 rotation;
    public Text senText;

    //for the UI slider
    public float Sensitivity { set => sensitivity = value; }

	IEnumerator Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		rotation = transform.localEulerAngles + body.parent.localEulerAngles;

		if (body.localPosition != Vector3.zero)
		{
			Debug.LogWarning("Body (child transform of player) must have a localPosition of 0,0,0. Resetting...");
			body.localPosition = Vector3.zero;
		}

		yield return new WaitForFixedUpdate();//rotation adjustments don't work without this
		Quaternion quaternion = body.parent.rotation;//rotation of root object
		body.parent.rotation = Quaternion.identity;
		body.rotation = quaternion;//apply any pre-existing rotation to the camera
	}

	void Update()
	{
		rotation.x = Mathf.Clamp(rotation.x - (Input.GetAxis("Mouse Y") * sensitivity), minAngle, maxAngle);
		rotation.y += Input.GetAxisRaw("Mouse X") * sensitivity % 360;
		transform.localEulerAngles = rotation + rotationOffset;
		body.localEulerAngles = new Vector3(0, rotation.y);

		if (Input.GetKeyDown(KeyCode.Tab)) Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
          senText.text = sensitivity.ToString();

    }
}
