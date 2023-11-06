using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{
    public void DestroyObjectWithTag(string tag)
    {
        GameObject objectToDestroy = GameObject.FindGameObjectWithTag(tag);
        if (objectToDestroy != null )
        {
            Destroy(objectToDestroy);
        }
    }
}
