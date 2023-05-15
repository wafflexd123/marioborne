using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPlus
{
	public bool canReflect;
	public float speed, maxLifetime;
	public Vector3 Direction { get => _direction; set { if (value != Vector3.negativeInfinity && value != Vector3.positiveInfinity && value != NAN_VECTOR) _direction = value;} }
	Vector3 _direction;
	Humanoid shooter;
	float timer;
	bool reflected;
	static readonly Vector3 NAN_VECTOR = new Vector3(float.NaN, float.NaN, float.NaN);

	public Bullet Initialise(float speed, Vector3 direction, Humanoid shooter)
	{
		this.speed = speed;
		this.Direction = direction;
		this.shooter = shooter;
		return this;
	}

	private void FixedUpdate()
	{
		transform.rotation = Quaternion.LookRotation(Direction);
		transform.position += speed * Time.fixedDeltaTime * Direction;
		timer += Time.fixedDeltaTime;
		if (timer >= maxLifetime) Destroy(gameObject);
	}

	private void OnTriggerEnter(Collider other)//only for bullet reflect surfaces on player
	{
		if (canReflect && FindComponent(other.transform, out BulletReflectSurface brs))
		{
			if (brs.enableReflect && FindComponent(brs.transform, out Humanoid humanoid))
			{
				Direction = humanoid.LookDirection;
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (canReflect && FindComponent(collision.collider.transform, out BulletReflectSurface brs) && brs.enableReflect)//look for reflection surface first
		{
			reflected = true;
			if (FindComponent(brs.transform, out Humanoid humanoid)) Direction = humanoid.LookDirection;
			else Direction = Vector3.Reflect(Direction, collision.contacts[0].normal);
		}
		else if (FindComponent(collision.collider.transform, out Humanoid human))
		{
			if (human != shooter || reflected)//if humanoid hit isn't the shooter, or the bullet has been reflected
			{
				human.Kill(DeathType.Bullet);
				Destroy(gameObject);
			}
		}
		else Destroy(gameObject);
	}
}
