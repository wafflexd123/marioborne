using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRewindable : MonoBehaviourPlus, IRewindListener
{
	float targetSeconds, maxPositions, rewindTimer = 0, windTimer = 0;
	readonly List<PositionAndVelocity> positions = new List<PositionAndVelocity>();
	PositionAndVelocity lastPos;
	new Rigidbody rigidbody;

	void Awake()
	{
		targetSeconds = 1f / Time.targetFrameRate;
		maxPositions = Time.targetFrameRate * Time.maxRewindTime;
		rigidbody = GetComponent<Rigidbody>();
		Time.rewindListeners.Add(this);
	}

	void Update()
	{
		windTimer += Time.deltaTime;
		if (windTimer >= targetSeconds)
		{
			if (positions.Count > maxPositions) positions.RemoveAt(0);
			positions.Add(new PositionAndVelocity(transform, rigidbody.velocity));
			windTimer = 0;
		}
	}

	public void StartRewind()
	{
		rewindTimer = 0;
		enabled = false;
		if (rigidbody != null) rigidbody.isKinematic = true;
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
				lastPos.position.ApplyToTransform(transform);
				rewindTimer = 0;
			}
		}
	}

	public void StopRewind()
	{
		enabled = true;
		if (rigidbody != null)
		{
			rigidbody.isKinematic = false;
			rigidbody.velocity = lastPos.velocity;
		}
	}

	private void OnDestroy()
	{
		Time.rewindListeners.Remove(this);
	}

	struct PositionAndVelocity
	{
		public PositionAndScale position;
		public Vector3 velocity;

		public PositionAndVelocity(PositionAndScale position, Vector3 velocity)
		{
			this.position = position;
			this.velocity = velocity;
		}
	}
}
