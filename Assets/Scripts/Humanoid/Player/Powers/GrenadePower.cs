using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadePower : MonoBehaviour, IPlayerPower
{
    private Player player;
    [SerializeField] private GameObject grenadePrefab;
    [Header("Throw Stats")]
    [SerializeField] private float throwForce = 30f;
    [SerializeField] private Vector3 throwOffset;

    public bool CanDisable => true; // TODO

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Ability")) ThrowGrenade();
    }

    private void ThrowGrenade()
    {
        GameObject grenadeObj = Instantiate(grenadePrefab, transform.position + throwOffset, Quaternion.identity);
        GrenadeObject grenadeObject = grenadeObj.GetComponent<GrenadeObject>();
        Rigidbody rb = grenadeObj.GetComponent<Rigidbody>();
        rb.AddForce(throwForce * player.LookDirection);
    }
}
