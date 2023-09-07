using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ColorLerper : MonoBehaviour
{
	public EasingFunction.Enum easingFunction;
	[field: SerializeField] public Color EndColor { get; set; }
	[field: SerializeField] public float TimeToLerp { get; set; }
	[field: SerializeField] public bool LerpOnEnable { get; set; }
	public UnityEvent onFinish;
	new Renderer renderer;

	private void Awake()
	{
		renderer = GetComponent<Renderer>();
	}

	public void StartLerp()
	{
		StartCoroutine(Lerp());
		IEnumerator Lerp()
		{
			Color startColor = renderer.material.color;
			float timer = 0, percent, ease;
			Func<float, float> easingFunction = EasingFunction.Get(this.easingFunction);
			do
			{
				timer += Time.fixedDeltaTime;
				percent = timer / TimeToLerp;
				if (percent > 1) percent = 1;
				ease = easingFunction(percent);
				renderer.material.color = Color.LerpUnclamped(startColor, EndColor, ease);
				yield return new WaitForFixedUpdate();
			} while (percent < 1);
			onFinish?.Invoke();
		}
	}

	private void OnEnable()
	{
		if (LerpOnEnable) StartLerp();
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}
}
