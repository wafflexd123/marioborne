using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPlus
{
	public bool canReflect;
	public float speed, maxLifetime;
	public Vector3 direction;
	Humanoid shooter;
	float timer;

	public Bullet Initialise(float speed, Vector3 direction, Humanoid shooter)
	{
		this.speed = speed;
		this.direction = direction;
		this.shooter = shooter;
		transform.rotation = Quaternion.LookRotation(direction);
		return this;
	}

	//IEnumerator Start()
	//{
	//	yield return new WaitForFixedUpdate();
	//	isFirstFrame = false;//only called once; so we don't have to continously set this every update
	//}

	private void FixedUpdate()
	{
		transform.position += speed * Time.fixedDeltaTime * direction;
		timer += Time.fixedDeltaTime;
		if (timer >= maxLifetime) Destroy(gameObject);
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
			if (human != shooter)//prevent bullet from killing the guy that shot it
			{
				human.Kill(DeathType.Bullet);
				Destroy(gameObject);
			}
		}
		else Destroy(gameObject);
	}
}
