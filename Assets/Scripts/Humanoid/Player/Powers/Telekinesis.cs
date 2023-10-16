using UnityEngine;
using UnityEngine.UI;

public class Telekinesis : MonoBehaviourPlus, IPlayerPower
{
	[Header("Telekinetic Lift Variables")]
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
	[SerializeField] public static float KillingSpeedThreshold = 15f;

	[Header("Telekinetic Push Charge Variables")]
	[SerializeField] private float maxChargeTime = 2.0f;
	[SerializeField] private float minPushStrength = 10.0f;

	[Header("UI Elements")]
	public GameObject sliderObject;
	public GameObject rangeIndicatorObject;
	public Slider chargeSlider;
	public Image chargeSliderFill;
    public Image rangeIndicator;
    public Color maxChargeColor = Color.red;
	public Color minChargeColor = Color.blue;
	public Color inRangeColor = Color.green;
	private bool inRange = false;

	//Script
	private ITelekinetic grabbedObject;
	private ITelekinetic hoveredObject;
	private float objectDistance;
	private float chargeTime;
	private Camera mainCamera;
	private RaycastHit raycast;

	public bool CanDisable => true;

	void Start()
	{
		mainCamera = Player.singlePlayer.camera;
	}

	void Update()
	{
		if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out raycast, grabDistance) && FindComponent(raycast.transform, out hoveredObject))
		{
			inRange = true;
		}
		else
		{
			inRange = false;
		}
		UpdateRangeUI();
		if (Input.GetButton("Ability"))
		{
			chargeTime += Time.deltaTime;
			if (chargeTime > maxChargeTime) chargeTime = maxChargeTime;
			HandLeftManager.Instance.SetEnergy(chargeTime / maxChargeTime);
			UpdateChargeUI();
		}
		else if (Input.GetButtonUp("Ability"))
		{
			if (chargeTime < 0.2f)  // Threshold to differentiate between click and hold
			{
				if (grabbedObject != null)
				{
					ReleaseObject();
				}
				else if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out raycast, grabDistance) && FindComponent(raycast.transform, out grabbedObject))
				{
					grabbedObject.TelekineticGrab(this);
					objectDistance = Vector3.Distance(mainCamera.transform.position, grabbedObject.transform.position);
				}
			}
			else
			{
				float chargedPushStrength = Mathf.Lerp(minPushStrength, pushStrength, chargeTime / maxChargeTime);
				if (grabbedObject != null) PushGrabbedObject(chargedPushStrength);
				else PushObjects(chargedPushStrength);
			}
		}
		if (grabbedObject != null) ControlObject();
	}

	public void ReleaseObject()
	{
		grabbedObject.TelekineticRelease();
		grabbedObject = null;
		objectDistance = 0;
		StopCharge();
	}

	void StopCharge()
	{
		chargeTime = 0;
		HandLeftManager.Instance.SetEnergy(0);
		UpdateChargeUI();
	}

    void ControlObject()
    {
        // Changing distance based on scroll wheel
        objectDistance -= Input.mouseScrollDelta.y * -scrollSpeed;
        objectDistance = Mathf.Clamp(objectDistance, minDistance, maxDistance);

        // Calculate target position based on ray from camera through mouse pointer
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition = ray.GetPoint(objectDistance);

        // Adding wobble
        targetPosition += Mathf.Sin(UnityEngine.Time.time * wobbleFrequency) * wobbleAmplitude * mainCamera.transform.up;

        float offset = 0f;

        MeshRenderer meshRenderer = grabbedObject.transform.GetComponent<MeshRenderer>();
        CapsuleCollider capsuleCollider = grabbedObject.transform.GetComponent<CapsuleCollider>();

        if (meshRenderer != null)
        {
            offset = meshRenderer.bounds.extents.magnitude;
        }
        else if (capsuleCollider != null)
        {
            offset = capsuleCollider.radius;
        }

        if (Physics.Linecast(grabbedObject.transform.position, targetPosition, out RaycastHit hit))
        {
            if (hit.collider.gameObject != grabbedObject.gameObject)
            {
                targetPosition = hit.point + hit.normal * offset;
            }
        }

        if (grabbedObject.gameObject.TryGetComponent(out Rigidbody rb))
        {
            Vector3 newPosition = Vector3.Lerp(rb.position, targetPosition, Time.deltaTime * followSpeed);
            rb.MovePosition(newPosition);
        }
    }

    void PushObjects(float strength)
	{
		Vector3 pushDirection = mainCamera.transform.forward;
		Vector3 boxCenter = mainCamera.transform.position + pushDirection * (pushLength / 2) + mainCamera.transform.up * (pushHeight / 6);

		Collider[] objectsToPush = Physics.OverlapBox(boxCenter, new Vector3(pushWidth / 2, pushHeight / 2, pushLength / 2), mainCamera.transform.rotation);

		foreach (Collider obj in objectsToPush)
		{
			if (FindComponent(obj.transform, out ITelekinetic _))
			{
				if (obj.TryGetComponent(out Rigidbody rb))
				{
					Vector3 adjustedPushDirection = pushDirection + Vector3.up * upwardsPushForce;
					adjustedPushDirection.Normalize();

					rb.AddForce(adjustedPushDirection * strength, ForceMode.Impulse);
				}
			}
		}

		StopCharge();
	}

	void PushGrabbedObject(float strength)
	{
		if (grabbedObject.gameObject.TryGetComponent(out Rigidbody rb))
		{
			Vector3 pushDirection = mainCamera.transform.forward;
			rb.AddForce(pushDirection.normalized * strength, ForceMode.Impulse);
			grabbedObject.gameObject.AddComponent<TelekineticObjectDamage>();
		}
		ReleaseObject();
	}

	void UpdateChargeUI()
	{
		float chargeRatio = chargeTime / maxChargeTime;
		chargeSlider.value = chargeRatio;
		chargeSliderFill.color = Color.Lerp(minChargeColor, maxChargeColor, chargeRatio);
	}

	void UpdateRangeUI()
	{
		if (inRange || grabbedObject != null) rangeIndicator.color = inRangeColor;
		else rangeIndicator.color = maxChargeColor;
	}

	private void OnEnable()
	{
		if (sliderObject != null) sliderObject.SetActive(true);
		if (rangeIndicatorObject != null) rangeIndicatorObject.SetActive(true);
	}

	private void OnDisable()
	{
		if (sliderObject != null) sliderObject.SetActive(false);
        if (rangeIndicatorObject != null) rangeIndicatorObject.SetActive(false);
        if (grabbedObject != null) ReleaseObject();
		else StopCharge();
	}

	//void OnDrawGizmos()
	//{
	//    if (mainCamera != null)
	//    {
	//        Vector3 pushDirection = mainCamera.transform.forward;
	//        Vector3 boxCenter = mainCamera.transform.position + pushDirection * (pushLength / 2) + mainCamera.transform.up * (pushHeight / 6);

	//        Gizmos.color = Color.red;
	//        Gizmos.matrix = Matrix4x4.TRS(boxCenter, mainCamera.transform.rotation, new Vector3(pushWidth, pushHeight, pushLength));
	//        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	//    }
	//}
}

public interface ITelekinetic
{
	public GameObject gameObject { get; }
	public Transform transform { get; }
	public void TelekineticGrab(Telekinesis t);
	public void TelekineticRelease();
}