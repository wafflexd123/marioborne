using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandFollowObject : MonoBehaviour
{
    public Transform target;
    private Vector3 vOffset;
    private Quaternion rOffset;
    void Start()
    {
        vOffset = transform.localPosition;
        rOffset = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;
        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    public void AddIKTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void RemoveIKTarget()
    {
        target = null;
        transform.localPosition = vOffset;
        transform.localRotation = rOffset;
    }
}
