using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeslowRigidbody : MonoBehaviour
{
	Vector3 gravity;
	Vector3 velocity;
	float magnitude;
	List<Force> forces = new List<Force>();
	new Rigidbody rigidbody;
	float drag, mass;

	public void ControlRigidbody(bool isGrounded)
	{
		for (int i = 0; i < forces.Count; i++)
		{
			switch (forces[i].forceMode)
			{
				case ForceMode.Force:
					velocity += forces[i].force / mass * Time.fixedDeltaTime;
					break;
				case ForceMode.Acceleration:
					velocity += forces[i].force * Time.fixedDeltaTime;
					break;
				case ForceMode.Impulse:
					velocity += forces[i].force / mass;
					break;
				case ForceMode.VelocityChange:
					velocity += forces[i].force;
					break;
			}
		}
		forces.Clear();
		if (!isGrounded) velocity += gravity * Time.fixedDeltaTime;
		velocity *= 1 - Time.fixedDeltaTime * drag;
		rigidbody.velocity = velocity * Time.timeScale;
	}
}

public struct Force
{
	public Vector3 force;
	public ForceMode forceMode;

	public Force(Vector3 force, ForceMode forceMode)
	{
		this.force = force;
		this.forceMode = forceMode;
	}
}
