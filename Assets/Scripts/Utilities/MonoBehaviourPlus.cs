using System;
using System.Collections;
using UnityEngine;
public class MonoBehaviourPlus : MonoBehaviour
{
	/// <summary>
	/// min = inclusive; max = exclusive
	/// </summary>
	public static bool InRange(int i, int min, int max)
	{
		return i >= min && i < max;
	}

	/// <summary>
	/// min = inclusive; max = inclusive
	/// </summary>
	public static bool InRange(float f, float min, float max)
	{
		return f >= min && f <= max;
	}

	public IEnumerator TweenFloat(Func<float> inFloat, Action<float> outFloat, float target, float speed)
	{
		if (inFloat() != target)
		{
			int direction = inFloat() > target ? -1 : 1;
			while (true)
			{
				outFloat(inFloat() + Time.fixedDeltaTime * speed * direction);
				if (inFloat() * direction >= target * direction)
				{
					outFloat(target);
					yield break;
				}
				yield return new WaitForFixedUpdate();
			}
		}
	}

	public float TweenFloat(float value, float target, float speed)
	{
		if (value != target)
		{
			int direction = value > target ? -1 : 1;
			value += Time.fixedDeltaTime * speed * direction;
			if (value * direction >= target * direction) value = target;
		}
		return value;
	}

	public static bool IsInLayer(int layerNumber, int layerMask)
	{
		return ((1 << layerNumber) & layerMask) != 0;
	}

	public static Vector3 VectorArray(float[] array)
	{
		return new Vector3(array[0], array[1], array[2]);
	}

	public static float[] VectorArray(Vector3 vector)
	{
		return new float[] { vector.x, vector.y, vector.z };
	}

	public static bool ApproxEquals(Vector3 a, Vector3 b, float errorMargin = 0.001f)
	{
		return (a - b).sqrMagnitude < errorMargin * errorMargin;
	}

	public IEnumerator MoveToPos(Position worldPosition, float speed, Transform transform = null, Action onEnd = null, float error = 0.001f)
	{
		if (transform == null) transform = this.transform;
		transform.eulerAngles = worldPosition.eulers;//temp
		while (!ApproxEquals(transform.position, worldPosition.coords, error))
		{
			transform.position = Vector3.MoveTowards(transform.position, worldPosition.coords, Time.fixedDeltaTime * speed);
			yield return new WaitForFixedUpdate();
		}
		transform.position = worldPosition.coords;
		onEnd?.Invoke();
	}

	public IEnumerator MoveToPosLocal(Position localPosition, float speed, Transform transform = null, Action onEnd = null, float error = 0.001f)
	{
		if (transform == null) transform = this.transform;
		transform.localEulerAngles = localPosition.eulers;//temp
		while (!ApproxEquals(transform.localPosition, localPosition.coords, error))
		{
			transform.localPosition = Vector3.MoveTowards(transform.localPosition, localPosition.coords, Time.fixedDeltaTime * speed);
			yield return new WaitForFixedUpdate();
		}
		transform.localPosition = localPosition.coords;
		onEnd?.Invoke();
	}

	public IEnumerator LerpToPosRewindable(PositionAndScale worldPosition, float time, Func<float> rewindSeconds, Transform transform = null, Action onEnd = null, Action undoOnEnd = null, Action onCoroutineExit = null, Func<float, float> easingFunction = null)
	{
		if (transform == null) transform = this.transform;
		if (easingFunction == null) easingFunction = EasingFunction.Linear;
		PositionAndScale startPosition = transform;
		float percent, currentRewind, ease, timer = 0, lastPercent = 0, overTime = Time.maxRewindTime + time;
		bool ended = false;
		do
		{
			//timer:
			if ((currentRewind = rewindSeconds()) != 0)//if we are in a rewind
			{
				timer -= currentRewind;
				if (timer < 0) timer = 0;
			}
			else timer += Time.deltaTime;//not in rewind

			//eased movement
			percent = Mathf.Clamp01(timer / time);
			if (percent != lastPercent)//dont keep trying to lerp in overtime
			{
				lastPercent = percent;
				ease = easingFunction(percent);
				transform.eulerAngles = Vector3.LerpUnclamped(startPosition.eulers, worldPosition.eulers, ease);
				transform.position = Vector3.LerpUnclamped(startPosition.coords, worldPosition.coords, ease);
				transform.localScale = Vector3.LerpUnclamped(startPosition.scale, worldPosition.scale, ease);
			}

			//end handling
			if (timer >= time)//if finished lerping
			{
				if (!ended)
				{
					onEnd?.Invoke();
					ended = true;
				}
			}
			else //if still lerping (timer < time)
			{
				if (ended)
				{
					undoOnEnd?.Invoke();
					ended = false;
				}

				if (timer == 0) break;//if rewound to beginning of lerp
			}

			yield return null;

		} while (timer < overTime);//go ~20 seconds over, so we can catch a rewind
		onCoroutineExit?.Invoke();
	}

	public IEnumerator LerpToPos(PositionAndScale worldPosition, float time, Transform transform = null, Action onEnd = null, Func<float, float> easingFunction = null)
	{
		if (transform == null) transform = this.transform;
		if (easingFunction == null) easingFunction = EasingFunction.Linear;
		PositionAndScale startPosition = new PositionAndScale(transform);
		float timer = 0, percent, ease;
		do
		{
			timer += Time.fixedDeltaTime;
			percent = timer / time;
			if (percent > 1) percent = 1;
			ease = easingFunction(percent);
			transform.eulerAngles = Vector3.LerpUnclamped(startPosition.eulers, worldPosition.eulers, ease);
			transform.position = Vector3.LerpUnclamped(startPosition.coords, worldPosition.coords, ease);
			transform.localScale = Vector3.LerpUnclamped(startPosition.scale, worldPosition.scale, ease);
			yield return new WaitForFixedUpdate();
		} while (percent < 1);
		onEnd?.Invoke();
	}

	public IEnumerator LerpToPosLocal(PositionAndScale localPosition, float time, Transform transform = null, Action onEnd = null, Func<float, float> easingFunction = null)
	{
		if (transform == null) transform = this.transform;
		if (easingFunction == null) easingFunction = EasingFunction.Linear;
		PositionAndScale startPosition = new PositionAndScale(transform, true);
		float timer = 0, percent, ease;
		do
		{
			timer += Time.fixedDeltaTime;
			percent = timer / time;
			if (percent > 1) percent = 1;
			ease = easingFunction(percent);
			transform.localEulerAngles = Vector3.LerpUnclamped(startPosition.eulers, localPosition.eulers, ease);
			transform.localPosition = Vector3.LerpUnclamped(startPosition.coords, localPosition.coords, ease);
			transform.localScale = Vector3.LerpUnclamped(startPosition.scale, localPosition.scale, ease);
			yield return new WaitForFixedUpdate();
		} while (percent < 1);
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

	public void StopCoroutine(ref Coroutine variable)
	{
		if (variable != null)
		{
			StopCoroutine(variable);
			variable = null;
		}
	}

	/// <summary>
	/// Iterates through transform's parents to component
	/// </summary>
	public static bool FindComponent<T>(Transform transform, out T result)
	{
		while (transform != null)
		{
			if (transform.TryGetComponent(out result)) return true;
			transform = transform.parent;
		}
		result = default;
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

		public Position(float x, float y, float z)
		{
			this.coords = new Vector3(x, y, z);
			this.eulers = Vector3.zero;
		}

		public Position(Transform transform, bool useLocal = false)
		{
			this.coords = useLocal ? transform.localPosition : transform.position;
			this.eulers = useLocal ? transform.localEulerAngles : transform.eulerAngles;
		}

		public void ApplyToTransform(Transform t, bool useLocal = false)
		{
			if (useLocal)
			{
				t.localEulerAngles = coords;
				t.localEulerAngles = eulers;
			}
			else
			{
				t.position = coords;
				t.eulerAngles = eulers;
			}
		}

		public static implicit operator Vector3(Position p) => p.coords;
		public static implicit operator Quaternion(Position p) => Quaternion.Euler(p.eulers);
		public static implicit operator Position(Transform p) => new Position(p);
		public static implicit operator Position(PositionAndScale p) => new Position(p.coords, p.eulers);
		public static Position operator +(Position a, Position b) => new Position(a.coords + b.coords, a.eulers + b.eulers);
		public static Position operator -(Position a, Position b) => new Position(a.coords - b.coords, a.eulers - b.eulers);
	}

	[Serializable]
	public struct PositionAndScale
	{
		public Vector3 coords, eulers, scale;

		public PositionAndScale(Vector3 coords, Vector3 eulers, Vector3 scale)
		{
			this.coords = coords;
			this.eulers = eulers;
			this.scale = scale;
		}

		public PositionAndScale(Transform transform, bool useLocal = false)
		{
			if (useLocal)
			{
				this.coords = transform.localPosition;
				this.eulers = transform.localEulerAngles;
				this.scale = transform.localScale;
			}
			else
			{
				this.coords = transform.position;
				this.eulers = transform.eulerAngles;
				this.scale = transform.lossyScale;
			}
		}

		public void ApplyToTransform(Transform t, bool useLocal = false)
		{
			if (useLocal)
			{
				t.localPosition = coords;
				t.localEulerAngles = eulers;
				t.localScale = scale;
			}
			else
			{
				t.position = coords;
				t.eulerAngles = eulers;
				t.localScale = scale;//might not work
			}
		}

		public static implicit operator Vector3(PositionAndScale p) => p.coords;
		public static implicit operator Quaternion(PositionAndScale p) => Quaternion.Euler(p.eulers);
		public static implicit operator PositionAndScale(Transform p) => new PositionAndScale(p);
		public static implicit operator PositionAndScale(Position p) => new PositionAndScale(p.coords, p.eulers, Vector3.one);
		public static PositionAndScale operator +(PositionAndScale a, PositionAndScale b) => new PositionAndScale(a.coords + b.coords, a.eulers + b.eulers, a.scale + b.scale);
		public static PositionAndScale operator -(PositionAndScale a, PositionAndScale b) => new PositionAndScale(a.coords - b.coords, a.eulers - b.eulers, a.scale - b.scale);
	}
}
