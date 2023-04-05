using System;
using System.Collections;
using UnityEngine;
public class MonoBehaviourPlus : MonoBehaviour
{
	public static Vector3 VectorArray(float[] array)
	{
		return new Vector3(array[0], array[1], array[2]);
	}

	public static float[] VectorArray(Vector3 vector)
	{
		return new float[] { vector.x, vector.y, vector.z };
	}

	public static bool ApproxEquals(Vector3 a, Vector3 b, float sqrErrorMargin = 0.0000001f)
	{
		return (a - b).sqrMagnitude < sqrErrorMargin;
	}

	public IEnumerator LerpToPos(Transform transformToLerp, Position pos, float speed, Action onEnd = null)
	{
		transformToLerp.localEulerAngles = pos.eulers;//temp
		while (!ApproxEquals(transformToLerp.localPosition, pos.coords, 0.0001f))
		{
			transformToLerp.localPosition = Vector3.MoveTowards(transformToLerp.localPosition, pos.coords, Time.deltaTime * speed);
			yield return null;
		}
		transformToLerp.localPosition = pos.coords;
		onEnd?.Invoke();
	}

	public void ResetRoutine(IEnumerator routine, ref Coroutine variable)
	{
		if (variable != null)
		{
			StopCoroutine(variable);
		}
		variable = StartCoroutine(routine);
	}

	public void StopRoutine(ref Coroutine variable)
	{
		if (variable != null)
		{
			StopCoroutine(variable);
			variable = null;
		}
	}

	/// <summary>
	/// Iterates through transform's parents to component in layer
	/// </summary>
	public static bool FindComponent<T>(int layerMask, Transform transform, out T result) where T : MonoBehaviour
	{
		while (transform != null)
		{
			if (((1 << transform.gameObject.layer) & layerMask) != 0)
			{
				if (transform.TryGetComponent<T>(out result)) return true;
			}
			transform = transform.parent;
		}
		result = null;
		return false;
	}

	/// <summary>
	/// Iterates through transform's parents to component with tag
	/// </summary>
	public static bool FindComponent<T>(string tag, Transform transform, out T result) where T : MonoBehaviour
	{
		while (transform != null)
		{
			if (transform.CompareTag(tag))
			{
				if (transform.TryGetComponent<T>(out result)) return true;
			}
			transform = transform.parent;
		}
		result = null;
		return false;
	}

	/// <summary>
	/// Iterates through transform's parents to component
	/// </summary>
	public static bool FindComponent<T>(Transform transform, out T result) where T : MonoBehaviour
	{
		while (transform != null)
		{
			if (transform.TryGetComponent<T>(out result)) return true;
			transform = transform.parent;
		}
		result = null;
		return false;
	}

	[System.Serializable]
	public struct Position
	{
		public Vector3 coords, eulers;

		public Position(Vector3 coords, Vector3 eulers)
		{
			this.coords = coords;
			this.eulers = eulers;
		}

		public Position(Transform transform, bool useLocal = false)
		{
			this.coords = useLocal ? transform.localPosition : transform.position;
			this.eulers = useLocal ? transform.localEulerAngles : transform.eulerAngles;
		}
	}
}
