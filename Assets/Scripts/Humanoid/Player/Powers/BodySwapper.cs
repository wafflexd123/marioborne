using UnityEngine;

public class BodySwapper : MonoBehaviourPlus
{
	public float teleportSpeed;
	Player player;

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
				player.TeleportToEnemy(e, teleportSpeed);
			}
		}
	}
}
