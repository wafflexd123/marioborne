using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRewinder : MonoBehaviourPlus, IRewindListener
{
	float targetSeconds, maxPositions, rewindTimer = 0, windTimer = 0;
	readonly List<PositionAndVelocity> positions = new List<PositionAndVelocity>();
	PositionAndVelocity lastPos;
	new Rigidbody rigidbody;
	PlayerMovement player;

	void Awake()
	{
		targetSeconds = 1f / Time.targetFrameRate;
		maxPositions = Time.targetFrameRate * Time.maxRewindTime;
		rigidbody = GetComponent<Rigidbody>();
		player = GetComponent<PlayerMovement>();
		Time.rewindListeners.Add(this);
	}

	void Update()
	{
		windTimer += Time.deltaTime;
		if (windTimer >= targetSeconds)
		{
			if (positions.Count > maxPositions) positions.RemoveAt(0);
			positions.Add(new PositionAndVelocity(transform.position, player.xzVelocity.vector, player.yVelocity.vector));
			windTimer = 0;
		}
	}

	public void StartRewind()
	{
		rewindTimer = 0;
		enabled = false;
		player.enabled = false;
		rigidbody.isKinematic = true;
	}

	public void Rewind(float seconds)
	{
		if (positions.Count > 0)
		{
			rewindTimer += seconds;
			if (rewindTimer >= targetSeconds)
			{
				lastPos = positions[^1];
				positions.RemoveAt(positions.Count - 1);
				transform.position = lastPos.position;
				rewindTimer = 0;
			}
		}
	}

	public void StopRewind()
	{
		enabled = true;
		player.enabled = true;
		rigidbody.isKinematic = false;
		player.xzVelocity.vector = lastPos.xzVelocity;
		player.yVelocity.vector = lastPos.yVelocity;
		rigidbody.velocity = lastPos.xzVelocity + lastPos.yVelocity;
	}

	private void OnDestroy()
	{
		Time.rewindListeners.Remove(this);
	}

	struct PositionAndVelocity
	{
		public Vector3 position, xzVelocity, yVelocity;

		public PositionAndVelocity(Vector3 position, Vector3 xzVelocity, Vector3 yVelocity)
		{
			this.position = position;
			this.xzVelocity = xzVelocity;
			this.yVelocity = yVelocity;
		}
	}
}
