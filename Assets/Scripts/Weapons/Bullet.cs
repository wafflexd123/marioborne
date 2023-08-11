using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPlus
{
	//Inspector
	public float maxLifetime;

	//Script
	[HideInInspector] public Vector3 direction;
	[HideInInspector] public float speed;
	public Type shooterType;
	float timer;
	new ParticleSystem particleSystem;
	new Renderer renderer;

	public Color color
	{
		set
		{
			ParticleSystem.MainModule p = particleSystem.main;
			p.startColor = value;
			renderer.material.color = value;
		}
		get => renderer.material.color;
	}

	private void Awake()
	{
		particleSystem = transform.Find("Bullet Trail").GetComponent<ParticleSystem>();
		renderer = transform.Find("Model").GetComponent<Renderer>();
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
		transform.rotation = Quaternion.LookRotation(direction);
		transform.position += speed * Time.fixedDeltaTime * direction;
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
	}

	private void OnDisable()
	{
		GetComponent<Rigidbody>().isKinematic = true;
		transform.Find("Model").GetComponent<Collider>().enabled = false;
	}
}

public interface IBulletReceiver
{
	public void OnBulletHit(Collision collision, Bullet bullet);
}
