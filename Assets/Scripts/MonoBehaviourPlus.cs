using UnityEngine;
using System.Collections;

[SelectionBase]
public class MonoBehaviourPlus : MonoBehaviour
{
	public static bool GetAxisRaw(string axis, out float value)
	{
		value = Input.GetAxisRaw(axis);
		return value != 0;
	}

	public static bool GetAxis(string axis, out float value)
	{
		value = Input.GetAxis(axis);
		return value != 0;
	}

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

	public IEnumerator LerpToPos(Transform transform, Position pos, float speed)
	{
		transform.localEulerAngles = pos.eulers;//temp
		while (!ApproxEquals(transform.localPosition, pos.coords, 0.0001f))
		{
			transform.localPosition = Vector3.MoveTowards(transform.localPosition, pos.coords, Time.deltaTime * speed);
			yield return null;
		}
		transform.localPosition = pos.coords;
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
	}
}
