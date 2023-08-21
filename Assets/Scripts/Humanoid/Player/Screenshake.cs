using System.Collections;
using System;
using UnityEngine;

public class Screenshake : MonoBehaviour
{
	[SerializeField] AnimationCurve curve;
	[SerializeField] float strengthMultiplier, duration;
	[SerializeField] bool shakeOnEnable;
	Coroutine crtShake;
	Vector3 startPos;

	/// <summary>
	/// Shakes [obj] according to parameters supplied.
	/// </summary>
	/// <param name="obj">Object to shake</param>
	/// <param name="duration">Length in seconds of shake</param>
	/// <param name="strengthMultiplier">Value returned by the animation curve is multiplied by this</param>
	/// <param name="curve">Curve defining the strength of the animation over time. A good example is a curve that ramps up quickly to .2, and slowly dies back towards 0.</param>
	/// <returns>Shake coroutine; store this if you want to call StopCoroutine() prematurely. Use the Shake() instance overload instead if you want transform.position to automatically reset.</returns>
	public static Coroutine Shake(MonoBehaviour obj, Vector3 startPos, float duration, float strengthMultiplier, AnimationCurve curve, Action onEndShake = null)
	{
		return obj.StartCoroutine(E());
		IEnumerator E()
		{
			float time = 0;
			while (time < duration)
			{
				time += Time.deltaTime;
				obj.transform.position = startPos + (curve.Evaluate(time / duration) * strengthMultiplier * UnityEngine.Random.insideUnitSphere);
				yield return null;
			}
			obj.transform.position = startPos;
			onEndShake?.Invoke();
		}
	}

	/// <summary>
	/// Shakes gameobject if attached as a component according to inspector parameters
	/// </summary>
	public void Shake()
	{
		StopShake();
		crtShake = Shake(this, startPos = transform.position, duration, strengthMultiplier, curve, () => crtShake = null);
	}


	/// <summary>
	/// Stops shake if attached to a gameobject as a component. Automatically resets position back to the starting position.
	/// </summary>
	public void StopShake()
	{
		if (crtShake != null)
		{
			StopCoroutine(crtShake);
			transform.position = startPos;
			crtShake = null;
		}
	}

	private void OnEnable()
	{
		if (shakeOnEnable) Shake();
	}

	private void OnDisable()
	{
		StopShake();
	}
}
