using UnityEngine;

public class ConstantRotator : MonoBehaviour
{
    public float speed;
    public Vector3 axis;

    void Update()
    {
        transform.Rotate(axis, speed * Time.deltaTime);
    }
}
