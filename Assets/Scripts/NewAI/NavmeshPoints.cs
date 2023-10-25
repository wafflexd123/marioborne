using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavmeshPoints : MonoBehaviour
{
    public Vector3 gizmoSize = new Vector3(0.1f, 0.1f, 0.1f);
    public Color color;

    public void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Gizmos.color = color;
            Gizmos.DrawCube(new Vector3(transform.GetChild(i).transform.position.x, transform.GetChild(i).transform.position.y + gizmoSize.y / 2, transform.GetChild(i).transform.position.z), gizmoSize);
        }
    }
}
