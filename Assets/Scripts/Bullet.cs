using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPlus
{
	public float speed;
	public Vector3 direction;

	public Bullet Initialise(float speed, Vector3 direction)
	{
		this.speed = speed;
		this.direction = direction;
		transform.rotation = Quaternion.LookRotation(direction);
		return this;
	}

	void Update()
	{
		transform.position += Vector3.ClampMagnitude(direction * speed, speed * Time.deltaTime);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (FindComponent(collision.collider.transform, out BulletReflectSurface brs) && brs.enableReflect)
		{
			direction = Vector3.Reflect(direction, collision.contacts[0].normal);
		}
		else if (collision.gameObject.layer == 3)
			Destroy(gameObject);
	}
}
