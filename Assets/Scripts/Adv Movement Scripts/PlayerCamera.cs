using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCamera : MonoBehaviour
{
    public float sensitivity = 5.0f; // default sensitivity;
    public Transform body;
    Vector3 rotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UpdateSensitivity(float sensitivityValue)
    {
        Debug.Log("Slider value changed to: " + sensitivityValue);
        // 在这里更新敏感度
    }

    void Update()
    {
        rotation = new Vector3(Mathf.Clamp(rotation.x - (Input.GetAxis("Mouse Y") * sensitivity), -90, 90), (rotation.y + (Input.GetAxisRaw("Mouse X") * sensitivity)) % 360);
        transform.eulerAngles = rotation;
        body.localEulerAngles = new Vector3(0, rotation.y);
        float sensitivityMultiplier = GameObject.Find("Slider").GetComponent<Slider>().value;
        float sensitivityValue = sensitivity * sensitivityMultiplier;
    }
}
