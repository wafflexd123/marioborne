using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : BaseObject
{
	public float speed;
	public Vector3 direction;

	public Bullet Initialise(float speed, Vector2 direction)
	{
		this.speed = speed;
		this.direction = direction;
		return this;
	}

	void Update()
	{
		transform.position += Vector3.ClampMagnitude(direction * speed, speed * Time.deltaTime);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == 3)
			Destroy(gameObject);
		else if (FindComponent(collision.collider.transform, out BulletReflectSurface brs))
		{
			direction = Vector3.Reflect(direction, collision.contacts[0].normal);
		}
	}
}
