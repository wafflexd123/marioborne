using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevetateAndRotate : MonoBehaviour
{
    [SerializeField] private float rotationSpeed; 
    [SerializeField] private float amplitude;
    [SerializeField] private float period;
    [SerializeField] private Vector3 standardOffset = Vector3.zero;
    private Vector3 frameOffset = Vector3.zero;

    void Update()
    {
        //transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        frameOffset =  Vector3.up * (amplitude * Mathf.Sin(UnityEngine.Time.time * (2.0f*Mathf.PI / period)));
        transform.localPosition = standardOffset + frameOffset;
    }
}
