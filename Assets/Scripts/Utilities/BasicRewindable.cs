using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRewindable : MonoBehaviourPlus, IRewindListener
{
	public readonly List<Action> actionsThisFrame = new List<Action>();
	float targetSeconds, maxPositions, rewindTimer = 0, windTimer = 0;
	readonly List<PositionAndVelocity> positions = new List<PositionAndVelocity>();
	PositionAndVelocity lastPos;
	new Rigidbody rigidbody;

	protected virtual void Awake()
	{
		targetSeconds = 1f / Time.targetFrameRate;
		maxPositions = Time.targetFrameRate * Time.maxRewindTime;
		rigidbody = GetComponent<Rigidbody>();
		Time.rewindListeners.Add(this);
	}

	void LateUpdate()
	{
		windTimer += Time.deltaTime;
		if (windTimer >= targetSeconds)
		{
			if (positions.Count > maxPositions) positions.RemoveAt(0);
			positions.Add(GetPosition());
			actionsThisFrame.Clear();
			windTimer = 0;
		}
	}

	protected virtual PositionAndVelocity GetPosition()
	{
		return new PositionAndVelocity(transform, rigidbody != null ? rigidbody.velocity : Vector3.zero, actionsThisFrame);
	}

	public virtual void StartRewind()
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
				lastPos.ApplyPosition(transform);
				lastPos.CallActions();
				rewindTimer = 0;
			}
		}
	}

	public virtual void StopRewind()
	{
		enabled = true;
		if (rigidbody != null)
		{
			rigidbody.isKinematic = false;
			lastPos.ApplyVelocity(rigidbody);
		}
	}

	private void OnDestroy()
	{
		Time.rewindListeners.Remove(this);
	}

	protected class PositionAndVelocity
	{
		protected readonly PositionAndScale position;
		protected readonly Vector3 velocity;
		protected readonly List<Action> actions;

		public PositionAndVelocity(PositionAndScale position, Vector3 velocity, List<Action> actions)
		{
			this.position = position;
			this.velocity = velocity;
			this.actions = new List<Action>();
			this.actions.AddRange(actions);
		}

		public virtual void ApplyPosition(Transform t)
		{
			position.ApplyToTransform(t);
		}

		public virtual void ApplyVelocity(Rigidbody r)
		{
			r.velocity = velocity;
		}

		public virtual void CallActions()
		{
			foreach (Action item in actions) item();
		}
	}
}
