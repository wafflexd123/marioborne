using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float sensitivity;
    public Transform body;
    Vector3 rotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        rotation = new Vector3(Mathf.Clamp(rotation.x - (Input.GetAxis("Mouse Y") * sensitivity), -90, 90), (rotation.y + (Input.GetAxisRaw("Mouse X") * sensitivity)) % 360);
        transform.eulerAngles = rotation;
        body.localEulerAngles = new Vector3(0, rotation.y);
    }
}
