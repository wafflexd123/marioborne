using System.Collections;
using UnityEngine;

public class BodySwapBullet : MonoBehaviourPlus
{
	public float teleportSpeed, maxTeleportTime;
	[SerializeField] private Vector3 throwForce;
	[SerializeField] private float daggerGravity;
	private Vector3 gravityForce = Vector3.zero;
	//[SerializeField] private float daggerSpeed;
	private MeshRenderer mr;
	private TrailRenderer tr;
	private CapsuleCollider cap;
	private ParticleSystem particles;

	private Rigidbody rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		gravityForce = Vector3.up * -daggerGravity;
		mr = transform.GetChild(0).GetComponent<MeshRenderer>();
		tr = transform.GetChild(0).GetComponent<TrailRenderer>();
		cap = transform.GetChild(0).GetComponent<CapsuleCollider>();
		particles = transform.GetChild(1).GetComponent<ParticleSystem>();
	}

	public void Fire()
	{
		//throwForce = Player.singlePlayer.transform.TransformDirection(throwForce);
		throwForce = Player.singlePlayer.camera.transform.TransformDirection(throwForce) + Player.singlePlayer.Velocity;
		rb = GetComponent<Rigidbody>();
		rb.AddForce(throwForce, ForceMode.VelocityChange);
	}

	private void FixedUpdate()
	{
		rb.AddForce(gravityForce, ForceMode.Acceleration);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (FindComponent(collision.transform, out Humanoid man))
		{
			Player.singlePlayer.TeleportToEnemy(man, teleportSpeed, maxTeleportTime);
		}
		StartCoroutine(Destruction());
	}

	private IEnumerator Destruction()
	{
		mr.enabled = false;
		tr.emitting = false;
		cap.enabled = false;
		particles.Play();
		//Debug.Break();
		yield return new WaitForSeconds(2f);
		Destroy(gameObject);
	}

	//protected override void OnCollisionEnter(Collision collision)
	//{
	//    if (FindComponent(collision.transform, out Humanoid enemy) && enemy != shooter)
	//    {
	//        ((Player)shooter).TeleportToEnemy(enemy, teleportSpeed, maxTeleportTime);
	//    }
	//    Destroy(gameObject);
	//}
}
