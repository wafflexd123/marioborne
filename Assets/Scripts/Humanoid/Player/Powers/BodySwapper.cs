using UnityEngine;

public class BodySwapper : MonoBehaviourPlus
{
	public float teleportSpeed;
	Player player;
	Coroutine crtMoveToEnemy;

	private void Awake()
	{
		if (!FindComponent(transform, out player))
		{
			Debug.LogError("Bodyswapper could not find player!", this);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.E))//temporary
		{
			if (FindComponent(player.raycast.transform, out Enemy e))
			{
				TeleportToEnemy(e);
			}
		}
	}

	public void TeleportToEnemy(Enemy enemy)
	{
		if (enemy.enabled && crtMoveToEnemy == null)//dont teleport to dead/disabled enemies; will cause issues otherwise
		{
			Instantiate(player.model.deathPosePrefab, transform.position, transform.rotation);
			player.cameraController.enabled = false;
			player.movement.enabled = false;
			player.movement.ResetVelocity();
			player.movement.EnableCollider(false);
			enemy.enabled = false;
			if (player.weapon) player.weapon.Drop(0);
			crtMoveToEnemy = StartCoroutine(LerpToPos(new Position(enemy.transform), Vector3.Distance(enemy.transform.position, transform.position) / teleportSpeed, transform, () =>
			{
				if (enemy.weapon) enemy.weapon.Pickup(player, true);
				player.cameraController.enabled = true;
				player.movement.EnableCollider(true);
				player.movement.enabled = true;
				crtMoveToEnemy = null;
				Destroy(enemy.gameObject);
			}, EasingFunction.EaseInSine));
		}
	}
}
