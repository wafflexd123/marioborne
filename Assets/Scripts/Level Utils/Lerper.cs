using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lerper : UnityEventHelper, IRewindListener
{
	public EasingFunction.Enum easingFunction;
	[field: SerializeField] public float TimeToLerp { get; set; }
	[field: SerializeField] public Transform LerpTo { get; set; }
	[field: SerializeField] public bool LerpOnEnable { get; set; }
	[field: SerializeField] public bool Loop { get; set; }
	public UnityEvent onFinish;

	Position startPos;
	float rewindSeconds = 0;

	void Awake()
	{
		Time.rewindListeners.Add(this);
	}

	private void OnDestroy()
	{
		Time.rewindListeners.Remove(this);
	}

	public void StartLerp()
	{
		if (Loop)
		{
			startPos = transform;
			StartCoroutine(LerpToPos(LerpTo, TimeToLerp, transform, () => DirectionChange(true), EasingFunction.Get(easingFunction)));//loops dont rewind rn, probs dont need it yet
		}
		else StartCoroutine(LerpToPosRewindable(LerpTo, TimeToLerp, () => rewindSeconds, transform, () => onFinish.Invoke(), EasingFunction.Get(easingFunction)));
	}

	void DirectionChange(bool lerpBack)
	{
		if (lerpBack) StartCoroutine(LerpToPos(startPos, TimeToLerp, transform, () => DirectionChange(false), EasingFunction.Get(easingFunction)));
		else StartCoroutine(LerpToPos(LerpTo, TimeToLerp, transform, () => DirectionChange(true), EasingFunction.Get(easingFunction)));
	}

	private void OnEnable()
	{
		if (LerpOnEnable) StartLerp();
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	public void Rewind(float seconds)
	{
		rewindSeconds = seconds;
	}

	public void StartRewind()
	{
	}

	public void StopRewind()
	{
		rewindSeconds = 0;
	}
}
