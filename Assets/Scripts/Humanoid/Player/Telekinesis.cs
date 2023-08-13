using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telekinesis : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private LayerMask objectLayer;
    private RaycastHit raycast;
    [SerializeField] private float grabDistance = 100f;
    [SerializeField] private float followSpeed = 3.0f;
    [SerializeField] private float rotationSpeed = 30.0f;
    [SerializeField] private float wobbleFrequency = 1.0f;
    [SerializeField] private float wobbleAmplitude = 0.1f;
    [SerializeField] private float scrollSpeed = 1.0f;

    private GameObject grabbedObject;
    private bool isGrabbing;
    private Vector3 offset;
    private float objectDistance;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isGrabbing)
            {
                ReleaseObject();
            }
            else if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out raycast, grabDistance, objectLayer))
            {
                GrabObject(raycast.collider.gameObject);
            }
        }

        if (isGrabbing)
        {
            ControlObject(grabbedObject);
        }
    }

    void GrabObject(GameObject targetObject)
    {
        grabbedObject = targetObject;

        objectDistance = Vector3.Distance(mainCamera.transform.position, grabbedObject.transform.position);
        offset = grabbedObject.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, objectDistance));

        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
        }

        isGrabbing = true;
    }

    void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
            }

            grabbedObject = null;
        }

        isGrabbing = false;
    }

    void ControlObject(GameObject controlledObject)
    {
        // Changing distance based on scroll wheel
        objectDistance -= Input.mouseScrollDelta.y * -scrollSpeed;

        // Calculating targetposition based on mouse input
        Vector3 targetPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, objectDistance)) + offset;

        // Adding wobble
        targetPosition += mainCamera.transform.up * Mathf.Sin(UnityEngine.Time.time * wobbleFrequency) * wobbleAmplitude;

        // Move object towards target position
        controlledObject.transform.position = Vector3.Lerp(controlledObject.transform.position, targetPosition, UnityEngine.Time.deltaTime * followSpeed);

        // Apply rotation effect based on mouse movement
        Vector3 rotationDirection = (targetPosition - controlledObject.transform.position).normalized;
        controlledObject.transform.Rotate(rotationDirection, rotationSpeed * UnityEngine.Time.deltaTime);
    }

}
