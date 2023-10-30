using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientToMotion : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;
    private Quaternion quatOffset;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        quatOffset = Quaternion.Euler(rotationOffset);
    }

    void Update()
    {
        Vector3 direction = rb.velocity.normalized;
        Quaternion motionRot = Quaternion.LookRotation(direction);
        motionRot *= quatOffset;
        transform.rotation = motionRot;
    }
}
