using UnityEngine;
using UnityEngine.UI;

public class Telekinesis : MonoBehaviour, IPlayerPower
{
    private Camera mainCamera;
    private RaycastHit raycast;
    [Header("Telekinetic Lift Variables")]
    [SerializeField] private LayerMask objectLayer;
    [SerializeField] private float grabDistance = 100f;
    [SerializeField] private float followSpeed = 3.0f;
    [SerializeField] private float rotationSpeed = 30.0f;
    [SerializeField] private float wobbleFrequency = 1.0f;
    [SerializeField] private float wobbleAmplitude = 0.1f;
    [SerializeField] private float scrollSpeed = 1.0f;
    [SerializeField] private float minDistance = 3.0f;
    [SerializeField] private float maxDistance = 50.0f;

    [Header("Telekinetic Push Variables")]
    [SerializeField] private float pushStrength = 30.0f;
    [SerializeField] private float pushHeight = 5f;
    [SerializeField] private float pushWidth = 14f;
    [SerializeField] private float pushLength = 14f;
    [SerializeField] private float upwardsPushForce = 0.2f;

    [Header("Telekinetic Push Charge Variables")]
    [SerializeField] private float maxChargeTime = 2.0f;
    [SerializeField] private float minPushStrength = 10.0f;

    [Header("UI Elements")]
    public GameObject sliderObject;
    public Slider chargeSlider;
    public Image chargeSliderFill;
    public Color maxChargeColor = Color.red;
    public Color minChargeColor = Color.blue; 


    private GameObject grabbedObject;
    private bool isGrabbing;
    private bool isCharging;
    private Vector3 offset;
    private float objectDistance;
    private float chargeTime;
   
    public bool CanDisable => true;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Player.singlePlayer.camera;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Ability"))
        {
            isCharging = true;
            chargeTime = 0f;  // Reset charge time
        }

        if (Input.GetButtonUp("Ability"))
        {
            if (isCharging && chargeTime < 0.2f)  // Threshold to differentiate between click and hold
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
            else
            {
                float chargedPushStrength = Mathf.Lerp(minPushStrength, pushStrength, chargeTime / maxChargeTime);
                if (isGrabbing)
                {
                    PushGrabbedObject(chargedPushStrength);
                }
                else
                {
                    PushObjects(chargedPushStrength);
                }
            }

            isCharging = false;
        }

        if (isCharging)
        {
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
        }

        if (isGrabbing)
        {
            ControlObject(grabbedObject);
        }

        UpdateChargeUI();
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
        objectDistance = Mathf.Clamp(objectDistance, minDistance, maxDistance);

        // Calculating target position based on mouse input
        Vector3 targetPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, objectDistance)) + offset;

        // Adding wobble
        targetPosition += mainCamera.transform.up * Mathf.Sin(UnityEngine.Time.time * wobbleFrequency) * wobbleAmplitude;

        // Move object towards target position
        controlledObject.transform.position = Vector3.Lerp(controlledObject.transform.position, targetPosition, UnityEngine.Time.deltaTime * followSpeed);

        float currentRotationSpeed;
        if (isCharging)
        {
            // Increase rotation speed up to 100x when fully charged
            currentRotationSpeed = Mathf.Lerp(rotationSpeed, rotationSpeed * 100f, chargeTime / maxChargeTime);
        }
        else
        {
            currentRotationSpeed = rotationSpeed;
        }

        // Apply rotation effect based on mouse movement
        Vector3 rotationDirection = (targetPosition - controlledObject.transform.position).normalized;
        controlledObject.transform.Rotate(rotationDirection, currentRotationSpeed * UnityEngine.Time.deltaTime);

    }

    void PushObjects(float strength)
    {
        Vector3 pushDirection = mainCamera.transform.forward;
        Vector3 boxCenter = mainCamera.transform.position + pushDirection * (pushLength / 2) + mainCamera.transform.up * (pushHeight / 6);

        Collider[] objectsToPush = Physics.OverlapBox(boxCenter, new Vector3(pushWidth / 2, pushHeight / 2, pushLength / 2), mainCamera.transform.rotation, objectLayer);

        foreach (Collider obj in objectsToPush)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 adjustedPushDirection = pushDirection + Vector3.up * upwardsPushForce;
                adjustedPushDirection.Normalize();

                rb.AddForce(adjustedPushDirection * strength, ForceMode.Impulse);
            }
        }
    }

    void PushGrabbedObject(float strength)
    {
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDirection = mainCamera.transform.forward;
            rb.AddForce(pushDirection.normalized * strength, ForceMode.Impulse);
        }
        ReleaseObject();
    }

    void UpdateChargeUI()
    {
        if (isCharging)
        {
            float chargeRatio = chargeTime / maxChargeTime;
            chargeSlider.value = chargeRatio;

            chargeSliderFill.color = Color.Lerp(minChargeColor, maxChargeColor, chargeRatio);
        }
        else
        {
            chargeSlider.value = 0;
            chargeSliderFill.color = Color.clear;
        }
    }

    private void OnEnable()
    {
        if (sliderObject != null)
        {
            sliderObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (sliderObject != null)
        {
            sliderObject.SetActive(false);
        }
    }

    #region Debugging
    void OnDrawGizmos()
    {
        if (mainCamera != null)
        {
            Vector3 pushDirection = mainCamera.transform.forward;
            Vector3 boxCenter = mainCamera.transform.position + pushDirection * (pushLength / 2) + mainCamera.transform.up * (pushHeight / 6);

            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, mainCamera.transform.rotation, new Vector3(pushWidth, pushHeight, pushLength));
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
    #endregion
}
