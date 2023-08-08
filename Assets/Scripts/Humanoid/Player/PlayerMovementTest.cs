using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementTest : MonoBehaviour
{
	//velocity = maxVelocity * direction * evaluate(time)
	public float drag, maxVelocity, time;
	public AnimationCurve curve;
	Vector3 moveDirection, velocity;
	float timer;
	new Rigidbody rigidbody;
	new CapsuleCollider collider;
	Console.Line line;

	private void Start()
	{
		line = Console.AddLine("");
		rigidbody = GetComponent<Rigidbody>();
		collider = transform.Find("Body").GetComponent<CapsuleCollider>();
	}

	private void FixedUpdate()
	{
		float zInput = Input.GetAxis("Vertical"), xInput = Input.GetAxis("Horizontal");
		if (zInput != 0 || xInput != 0)
		{
			timer += Time.fixedDeltaTime;
			if (timer > time) timer = time;
			moveDirection = (collider.transform.forward * Input.GetAxis("Vertical") + collider.transform.right * Input.GetAxis("Horizontal")).normalized;
			velocity = maxVelocity * curve.Evaluate(timer / time) * moveDirection;
		}
		else
		{
			velocity *= 1 - Time.fixedDeltaTime * drag;
			timer = Mathf.Lerp(0, time, velocity.magnitude);
			if (timer < 0) timer = 0;
		}
		rigidbody.velocity = velocity;
		line.text = $"{timer}, {velocity}";
	}
}
