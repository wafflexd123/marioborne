using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float Sensitivity { set { sensX = value; sensY = value; } }

    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    [SerializeField] Transform cam;
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerTransform;

    [SerializeField] WallRunV2 wallRun;

    float mouseX;
    float mouseY;

    float multiplier = 0.01f;

    float xRotation;
    float yRotation;
    float zRotation;

    [SerializeField] AdvPlayerMovementV2 pm;

    private void Start()
    {
        Console.AddLine("Press TAB to lock/unlock mouse");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;

        MyInput();

        cam.transform.rotation = Quaternion.Euler(xRotation, yRotation, wallRun.tilt);
        orientation.transform.rotation = Quaternion.Euler(0, yRotation, zRotation);
        playerTransform.localRotation = Quaternion.Euler(0, yRotation, zRotation);
    }

    void MyInput()
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        yRotation += mouseX * sensX * multiplier;
        xRotation -= mouseY * sensY * multiplier;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }
}
