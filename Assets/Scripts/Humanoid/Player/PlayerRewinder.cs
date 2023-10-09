using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRewinder : BasicRewindable
{
	Player player;

	protected override void Awake()
	{
		base.Awake();
		player = GetComponent<Player>();
	}

	public override void StartRewind()
	{
		player.movement.enabled = false;
		base.StartRewind();
		player.input.enableInput = false;
		player.enabled = false;
	}

	public override void StopRewind()
	{
		base.StopRewind();
		player.input.enableInput = true;
		player.enabled = true;
		player.movement.enabled = true;
	}

	protected override BasicRewindable.PositionAndVelocity GetPosition()
	{
		return new PositionAndVelocity(player.movement, transform, player.movement.xzVelocity.vector, player.movement.yVelocity.vector, actionsThisFrame);
	}

	protected new class PositionAndVelocity : BasicRewindable.PositionAndVelocity
	{
		readonly Vector3 yVelocity;
		readonly PlayerMovement p;

		public PositionAndVelocity(PlayerMovement p, PositionAndScale position, Vector3 xzVelocity, Vector3 yVelocity, List<Action> actions) : base(position, xzVelocity, actions)
		{
			this.yVelocity = yVelocity;
			this.p = p;
		}

		public override void ApplyPosition(Transform t)
		{
			t.transform.position = position.coords;
		}

		public override void ApplyVelocity(Rigidbody r)
		{
			r.velocity = velocity + yVelocity;
			p.xzVelocity.vector = velocity;
			p.yVelocity.vector = yVelocity;
		}
	}
}
