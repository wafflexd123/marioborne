using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;

	void Update()
    {
        transform.position = cameraPosition.position;
    }
}
