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
		transform.rotation = Quaternion.LookRotation(direction);
		return this;
	}

	IEnumerator Start()
	{
		yield return null;
		isFirstFrame = false;//only called once; so we don't have to continously set this every update
	}

	private void Update()
	{
		transform.position += speed * Time.deltaTime * direction;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!isFirstFrame)//prevent bullet from insta-killing the person that shot it
		{
            if (canReflect && FindComponent(collision.collider.transform, out BulletReflectSurface brs))//look for reflection surface first (in case player/ai is holding one)
            {
                if (brs.enableReflect)
                {
                    if (collision.gameObject.GetComponentInParent<Player>())
                    {
                        direction = collision.gameObject.transform.forward;
                    }
                    else direction = Vector3.Reflect(direction, collision.contacts[0].normal);
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
