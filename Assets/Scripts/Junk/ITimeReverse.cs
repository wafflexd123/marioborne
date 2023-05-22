using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimeReverseObject : MonoBehaviourPlus
{
	Queue<Position> positions = new Queue<Position>();

	protected virtual void Awake()
	{
		StartCoroutine(SavePosition());
	}

	IEnumerator SavePosition()
	{
		positions.Enqueue(new Position(transform));
		yield return null;
	}

	public virtual void BeginReverse()
	{
		GetComponent<Rigidbody>().isKinematic = true;
	}

	public virtual void Reverse()
	{
		if (positions.Count > 0)
		{
			Position position = positions.Dequeue();
			transform.SetPositionAndRotation(position, position);
		}
	}

	public virtual void EndReverse()
	{
		GetComponent<Rigidbody>().isKinematic = false;
	}
}
