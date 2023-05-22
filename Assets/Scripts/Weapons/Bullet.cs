using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPlus
{
	public bool canReflect;
	public float speed, maxLifetime;
	public Color playerColor;
	public Vector3 Direction { get => _direction; set => _direction = value; }
	Vector3 _direction;
	Humanoid shooter;
	float timer;
	bool reflected;
	new ParticleSystem particleSystem;
	new Renderer renderer;

	private void Awake()
	{
		particleSystem = transform.Find("Bullet Trail").GetComponent<ParticleSystem>();
		renderer = transform.Find("Model").GetComponent<Renderer>();
	}

	public Bullet Initialise(float speed, Vector3 direction, Humanoid shooter)
	{
		this.speed = speed;
		this.Direction = direction;
		this.shooter = shooter;
		if (shooter is Player) SetColor(playerColor);
		return this;
	}

	private void FixedUpdate()
	{
		transform.rotation = Quaternion.LookRotation(Direction);
		transform.position += speed * Time.fixedDeltaTime * Direction;
		timer += Time.fixedDeltaTime;
		if (timer >= maxLifetime) Destroy(gameObject);
	}

	void SetColor(Color color)
	{
		ParticleSystem.MainModule p = particleSystem.main;
		p.startColor = color;
		renderer.material.color = color;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (canReflect && FindComponent(other.transform, out BulletReflectSurface brs) && brs.enableReflect)//if hit a reflect surface
		{
			if (FindComponent(brs.transform, out Humanoid humanoid)) Direction = humanoid.LookDirection;
			else Direction = -Direction;
			reflected = true;
			SetColor(playerColor);
		}
		else if (FindComponent(other.transform, out Humanoid human))//if hit a humanoid
		{
			if (reflected || human.GetType() != shooter.GetType())//if reflected || if shooter and target are not both AI or both a player
			{
				human.Kill(DeathType.Bullet);
				Destroy(gameObject);
			}
		}
		else Destroy(gameObject);//if hit a normal object
	}

	//private void OnCollisionEnter(Collision collision)
	//{
	//	if (canReflect && FindComponent(collision.collider.transform, out BulletReflectSurface brs) && brs.enableReflect)//look for reflection surface first
	//	{
	//		reflected = true;
	//		SetColor(playerColor);
	//		if (FindComponent(brs.transform, out Humanoid humanoid)) Direction = humanoid.LookDirection;
	//		else Direction = Vector3.Reflect(Direction, collision.contacts[0].normal);
	//	}
	//	else if (FindComponent(collision.collider.transform, out Humanoid human))
	//	{
	//		if (reflected || human.GetType() != shooter.GetType())//if reflected || if shooter and target are not both AI or both a player
	//		{
	//			human.Kill(DeathType.Bullet);
	//			Destroy(gameObject);
	//		}
	//	}
	//	else Destroy(gameObject);
	//}
}
