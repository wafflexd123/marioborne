using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazyFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float easingSpeed;
    [SerializeField] private float clampDistance;
    private Vector3 standardOffset = Vector3.zero;

    public delegate Vector3 offsetDelegate();
    public offsetDelegate offsetFunc = null;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        standardOffset = transform.position - target.position;
    }
    
    void LateUpdate()
    {
        Vector3 offset = standardOffset;
        if (offsetFunc != null) 
            offset += offsetFunc();

        //transform.position = Vector3.Lerp(transform.position, target.position + offset, easingSpeed * Time.deltaTime);
        transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, easingSpeed * Time.deltaTime);
        float distance = Vector3.Distance(transform.position, target.position + offset);
        if (distance > clampDistance)
        {
            print("Clamping distance");
            Vector3 direction = (transform.position - (target.position + offset)).normalized;
            transform.Translate(direction * (distance - clampDistance));
        }
    }
}
