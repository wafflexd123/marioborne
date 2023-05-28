using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviourPlus
{
	public int checkpointNumber;
	public bool debugSpawnHereOnAwake;
	private BoxCollider boxCollider;

	void Awake()
	{
		boxCollider = GetComponent<BoxCollider>();
		boxCollider.isTrigger = true;
		if (boxCollider.gameObject.layer == 0) boxCollider.gameObject.layer = 13;//ignore bullets layer
		if (boxCollider.excludeLayers == 0) boxCollider.excludeLayers = 1 << 13;//ignore bullets layer
		if (boxCollider.includeLayers == 0) boxCollider.includeLayers = ~(1 << 13);//ignore bullets layer
		if (debugSpawnHereOnAwake)
		{
			CheckpointManager.instance.lastCheckpointPos = transform.position;
			CheckpointManager.instance.lastCheckpoint = checkpointNumber;
			StartCoroutine(WaitToMovePlayer());
		}
	}

	IEnumerator WaitToMovePlayer()//doesn't work in awake idk why
	{
		yield return new WaitForFixedUpdate();
		Player.singlePlayer.transform.position = transform.position;
	}

	void OnTriggerEnter(Collider other)
	{
		if (FindComponent(other.transform, out Player _))
		{
			CheckpointManager.instance.lastCheckpointPos = transform.position;
			CheckpointManager.instance.lastCheckpoint = checkpointNumber;
		}
	}
}


