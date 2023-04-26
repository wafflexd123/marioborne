using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPlus
{
	public bool canReflect;
	public float speed;
	public Vector3 direction;
	bool isFirstFrame = true;

	public Bullet Initialise(float speed, Vector3 direction)
	{
		this.speed = speed;
		this.direction = direction;
		transform.LookAt(transform.position + direction);
		return this;
	}

	IEnumerator Start()
	{
		yield return new WaitForFixedUpdate();
		isFirstFrame = false;//only called once; so we don't have to continously set this every update
	}

	private void FixedUpdate()
	{
		transform.position += speed * Time.fixedDeltaTime * direction;
	}

	private void OnTriggerEnter(Collider other)//only for bullet reflect surfaces
	{
		if (canReflect && FindComponent(other.transform, out BulletReflectSurface brs))
		{
			if (brs.enableReflect && FindComponent(brs.transform, out Humanoid humanoid))
			{
				direction = humanoid.LookDirection;
				transform.LookAt(transform.position + direction);
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!isFirstFrame)//prevent bullet from insta-killing the person that shot it
		{
			if (canReflect && FindComponent(collision.collider.transform, out BulletReflectSurface brs))//look for reflection surface first
			{
				if (brs.enableReflect)
				{
					if (FindComponent(brs.transform, out Humanoid humanoid)) direction = humanoid.LookDirection;
					else direction = Vector3.Reflect(direction, collision.contacts[0].normal);
					transform.LookAt(transform.position + direction);
				}
			}
			else if (FindComponent(collision.collider.transform, out Humanoid human))
			{
				human.Kill();
				Destroy(gameObject);
			}
			else Destroy(gameObject);
		}
	}
}
