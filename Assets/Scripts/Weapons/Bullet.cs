using System;
using UnityEngine;

public class Bullet : MonoBehaviourPlus
{
	//Inspector
	public float maxLifetime;

	//Script
	[HideInInspector] public float speed;
	public Type shooterType;
	float timer;
	new ParticleSystem particleSystem;
	new Renderer renderer;
	new Rigidbody rigidbody;
	Vector3 _direction;

	public Vector3 direction
	{
		get => _direction;
		set
		{
			_direction = value;
			transform.rotation = Quaternion.LookRotation(_direction);
		}
	}

	public Color color
	{
		get => renderer.material.color;
		set
		{
			ParticleSystem.MainModule p = particleSystem.main;
			p.startColor = value;
			renderer.material.color = value;
		}
	}

	private void Awake()
	{
		particleSystem = transform.Find("Bullet Trail").GetComponent<ParticleSystem>();
		renderer = transform.Find("Model").GetComponent<Renderer>();
		rigidbody = GetComponent<Rigidbody>();
	}

	public Bullet Initialise(float speed, Vector3 direction, Humanoid shooter, Color color)
	{
		this.speed = speed;
		this.direction = direction;
		shooterType = shooter.GetType();
		this.color = color;
		enabled = true;
		return this;
	}

	private void FixedUpdate()
	{
		rigidbody.velocity = Time.timeScale * speed * direction;
		timer += Time.fixedDeltaTime;
		if (timer >= maxLifetime) Destroy(gameObject);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (FindComponent(collision.transform, out IBulletReceiver bulletReceiver))
		{
			bulletReceiver.OnBulletHit(collision, this);
		}
		else Destroy(gameObject);
	}

	private void OnEnable()
	{
		GetComponent<Rigidbody>().isKinematic = false;
		transform.Find("Model").GetComponent<Collider>().enabled = true;
		particleSystem.Play();
	}

	private void OnDisable()
	{
		GetComponent<Rigidbody>().isKinematic = true;
		transform.Find("Model").GetComponent<Collider>().enabled = false;
		particleSystem.Stop();
	}
}

public interface IBulletReceiver
{
	public void OnBulletHit(Collision collision, Bullet bullet);
}
