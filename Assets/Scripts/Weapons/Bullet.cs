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
    public bool reflected;

	public Bullet Initialise(float speed, Vector3 direction, Humanoid shooter)
	{
		this.speed = speed;
		this.direction = direction;
		this.shooter = shooter;
		return this;
	}

	//IEnumerator Start()
	//{
	//	yield return new WaitForFixedUpdate();
	//	isFirstFrame = false;//only called once; so we don't have to continously set this every update
	//}

	private void FixedUpdate()
	{
		transform.rotation = Quaternion.LookRotation(direction);
		transform.position += speed * Time.fixedDeltaTime * direction;
		timer += Time.fixedDeltaTime;
		if (timer >= maxLifetime) Destroy(gameObject);
	}

	private void OnTriggerEnter(Collider other)//only for bullet reflect surfaces on player
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
		Debug.Log(1);
		if (canReflect && FindComponent(collision.collider.transform, out BulletReflectSurface brs) && brs.enableReflect)//look for reflection surface first
		{
            reflected = true;
            if (FindComponent(brs.transform, out Humanoid humanoid)) direction = humanoid.LookDirection;
			else direction = Vector3.Reflect(direction, collision.contacts[0].normal);
			transform.LookAt(transform.position + direction);
		}
		else if (FindComponent(collision.collider.transform, out Humanoid human))
		{
			if (human != shooter && !reflected)//prevent bullet from killing the guy that shot it
			{
				human.Kill(DeathType.Bullet);
				Destroy(gameObject);
			}
            else
            {
                human.Kill(DeathType.Bullet);
                Destroy(gameObject);
            }
		}
		else Destroy(gameObject);
	}
}
