using UnityEngine;

public class RigidbodyStore
{
    public float mass, drag, angularDrag;
    public RigidbodyConstraints constraints;
    public RigidbodyInterpolation interpolation;
    public CollisionDetectionMode collisionDetectionMode;
    public bool isKinematic, useGravity;
    public Rigidbody rigidbody;

    public RigidbodyStore(Rigidbody rigidbody)
    {
        if (rigidbody != null)
        {
            mass = rigidbody.mass;
            drag = rigidbody.drag;
            angularDrag = rigidbody.angularDrag;
            constraints = rigidbody.constraints;
            interpolation = rigidbody.interpolation;
            collisionDetectionMode = rigidbody.collisionDetectionMode;
            useGravity = rigidbody.useGravity;
            isKinematic = rigidbody.isKinematic;
            this.rigidbody = rigidbody;
        }
    }

    public void Apply(Rigidbody rigidbody)
    {
        rigidbody.mass = mass;
        rigidbody.drag = drag;
        rigidbody.angularDrag = angularDrag;
        rigidbody.constraints = constraints;
        rigidbody.interpolation = interpolation;
        rigidbody.collisionDetectionMode = collisionDetectionMode;
        rigidbody.useGravity = useGravity;
        rigidbody.isKinematic = isKinematic;
        this.rigidbody = rigidbody;
    }
}
