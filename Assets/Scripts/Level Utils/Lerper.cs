using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lerper : UnityEventHelper
{
	public EasingFunction.Enum easingFunction;
	[field: SerializeField] public float TimeToLerp { get; set; }
	[field: SerializeField] public Transform LerpTo { get; set; }
	[field: SerializeField] public bool LerpOnEnable { get; set; }
	[field: SerializeField] public bool Loop { get; set; }
	public UnityEvent onFinish;

	Position startPos;

	public void StartLerp()
	{
		if (Loop) startPos = transform;
		StartCoroutine(LerpToPos(LerpTo, TimeToLerp, transform, Loop ? () => DirectionChange(true) : () => onFinish.Invoke(), EasingFunction.Get(easingFunction)));
	}

	void DirectionChange(bool lerpBack)
	{
		if (lerpBack) StartCoroutine(LerpToPos(startPos, TimeToLerp, transform, () => DirectionChange(false), global::EasingFunction.Get(easingFunction)));
		else StartCoroutine(LerpToPos(LerpTo, TimeToLerp, transform, () => DirectionChange(true), global::EasingFunction.Get(easingFunction)));
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
