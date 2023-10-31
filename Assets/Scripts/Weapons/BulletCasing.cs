using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCasing : MonoBehaviourPlus
{
	public AudioPool.Clips clips;
	public float lifeTime;
	float timer;
	bool hasPlayed;

	public void Spawn(Transform position, Vector3 forceDirection)
	{
		Instantiate(gameObject, position.position, position.rotation).GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.VelocityChange);
	}

	void Update()
	{
		timer += Time.deltaTime;
		if (timer >= lifeTime) Destroy(gameObject);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!hasPlayed && !FindComponent(collision.transform, out BulletCasing _))//dont play sound if hitting another casing mid-air
		{
			clips.PlayRandom(gameObject.AddComponent<AudioPool>().Initialise(1));
			hasPlayed = true;
		}
	}
}
