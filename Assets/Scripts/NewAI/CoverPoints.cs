using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverPoints : MonoBehaviour
{
    //public Transform coverPointsTransform;
    public Dictionary<Vector3, bool> coverPoints = new Dictionary<Vector3, bool>();

    public void Start()
    {
        InitializeCoverPoints();
    }

    public void InitializeCoverPoints()
    {
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            coverPoints.Add(transform.GetChild(i).position, false);
        }
    }

    public bool isCoverPointAvailable(Vector3 coverPoint)
    {
        if (coverPoints.ContainsKey(coverPoint))
        {
            return !coverPoints[coverPoint];
        }
        else
        {
            return false;
        }
    }

    public void MarkAsTaken(Vector3 coverPoint)
    {
        if (coverPoints.ContainsKey(coverPoint))
        {
            coverPoints[coverPoint] = true;
        }
    }

    public void MarkAsAvailable(Vector3 coverPoint)
    {
        if (coverPoints.ContainsKey(coverPoint))
        {
            coverPoints[coverPoint] = false;
        }
    }
}
