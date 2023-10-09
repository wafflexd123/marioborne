using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadePower : MonoBehaviour, IPlayerPower
{
    private Player player;
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private float chargeTime = 10f;
    //private float chargeTimeRemaining = 0f;
    private float thrownTime = -100f;
    [Header("Throw Stats")]
    [SerializeField] private float throwForce = 30f;
    [SerializeField] private float throwUpForce = 15f;
    [SerializeField] private Vector3 throwOffset;
    private bool inhand = true;

    private GrenadeObject grenadeObject;

    public bool CanDisable => true; // TODO

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    private void Start()
    {
        grenadeObject = GetComponentInChildren<GrenadeObject>();
    }

    private void Update()
    {
        CheckInHand();
        if (Input.GetButtonDown("Ability")) ThrowGrenade();
    }

    private void ThrowGrenade()
    {
        if (UnityEngine.Time.timeSinceLevelLoad - thrownTime >= chargeTime)
        {
            GameObject grenadeObj = Instantiate(grenadePrefab, transform.GetChild(0).position, Quaternion.identity);
            Rigidbody rb = grenadeObj.GetComponent<Rigidbody>();
            rb.AddForce(throwForce * player.LookDirection + throwUpForce * Vector3.up, ForceMode.Impulse);
            grenadeObject.Thrown();
            inhand = false;
            thrownTime = UnityEngine.Time.timeSinceLevelLoad;
        }
    }

    private void OnEnable()
    {
        CheckInHand();
    }

    private void CheckInHand()
    {
        if (inhand) return;

        if (UnityEngine.Time.timeSinceLevelLoad - thrownTime >= GrenadeLive.maxLifetime)
        {
            ReturnToHand(UnityEngine.Time.timeSinceLevelLoad);
        }
    }

    public void ReturnToHand(float startTime)
    {
        grenadeObject.gameObject.SetActive(true);
        grenadeObject.Reappear(startTime, chargeTime + thrownTime);
        inhand = true;
    }

	public void OnWeaponPickup()
	{
	}
}
