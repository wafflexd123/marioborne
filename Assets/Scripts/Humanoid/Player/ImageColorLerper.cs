using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ImageColorLerper : UnityEventHelper
{
	public EasingFunction.Enum easingFunction;
	[field: SerializeField] public Color StartColor { get; set; }
	[field: SerializeField] public Color EndColor { get; set; }
	[field: SerializeField] public float TimeToLerp { get; set; }
	[field: SerializeField] public float WaitTime { get; set; }
	[field: SerializeField] public bool LerpOnEnable { get; set; }
	public Graphic image;
	public UnityEvent onFinish;

	public void StartLerp()
	{
		StartCoroutine(Lerp());
		IEnumerator Lerp()
		{
			image.color = StartColor;
			yield return new WaitForSeconds(WaitTime);

			float timer = 0, percent, ease;
			Func<float, float> easingFunction = EasingFunction.Get(this.easingFunction);
			do
			{
				timer += Time.deltaTime;
				percent = timer / TimeToLerp;
				if (percent > 1) percent = 1;
				ease = easingFunction(percent);
				image.color = Color.LerpUnclamped(StartColor, EndColor, ease);
				yield return null;
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
