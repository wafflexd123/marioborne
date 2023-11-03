using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BasicRewindable))]
public class Bullet : MonoBehaviourPlus
{
	//Inspector
	public float maxLifetime;

	//Script
	[HideInInspector] public float speed;
	[HideInInspector] public MonoBehaviour shooter;
	[HideInInspector] public bool penetrates;
	private PlayerEnergy playerEnergy;
	float timer;
	//new ParticleSystem particleSystem;
	new Renderer renderer;
	new Rigidbody rigidbody;
	new Collider collider;
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
			//ParticleSystem.MainModule p = GetComponent<ParticleSystem>().main;
			//p.startColor = value;
			renderer.material.color = value;
		}
	}

	public virtual Bullet Spawn(Transform position, float speed, Vector3 direction, Humanoid shooter, Color color, bool penetrates)
	{
		Bullet b = Instantiate(gameObject, position.position, position.rotation).GetComponent<Bullet>();
		b.speed = speed;
		b.direction = direction;
		b.shooter = shooter;
		b.color = color;
		b.penetrates = penetrates;
		b.enabled = true;
		return b;
	}

	protected virtual void Awake()
	{
		//particleSystem = transform.Find("Trail").GetComponent<ParticleSystem>();
		renderer = transform.Find("Model").GetComponent<Renderer>();
		collider = renderer.GetComponent<Collider>();
		rigidbody = GetComponent<Rigidbody>();
		playerEnergy = GameObject.Find("/Player").GetComponent<PlayerEnergy>();
		GetComponent<BasicRewindable>().onFullyRewound.AddListener(() => Destroy(gameObject));
	}

	private void Update()
	{
		if (!Time.isRewinding)
		{
			timer += Time.deltaTime;
			if (timer >= maxLifetime) Destroy(gameObject);
		}
		else
		{
			timer -= Time.currentRewind;
		}
	}

	protected virtual void FixedUpdate()
	{
		if (!Time.isRewinding)
		{
			rigidbody.velocity = Time.timeScale * speed * direction;
		}
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		if (FindComponent(collision.transform, out IAttackReceiver bulletReceiver))
		{
			bulletReceiver.ReceiveAttack(shooter, this, DeathType.Bullet, collision);
			if (shooter.CompareTag("Player")) playerEnergy.IncreaseEnergy(10);
			if (!penetrates) Destroy(gameObject);
		}
		else Destroy(gameObject);
	}

	protected virtual void OnEnable()
	{
		rigidbody.isKinematic = false;
		collider.enabled = true;
		//particleSystem.Play();
	}

	protected virtual void OnDisable()
	{
		rigidbody.isKinematic = true;
		collider.enabled = false;
		//particleSystem.Stop();
	}
}
