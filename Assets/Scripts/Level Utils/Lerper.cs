using System;
using UnityEngine;
using UnityEngine.Events;

public class Lerper : UnityEventHelper, IRewindListener
{
	[field: SerializeField] public float TimeToLerp { get; set; }
	[field: SerializeField] public bool LerpOnEnable { get; set; }

	//These are handled by the custom inspector
	[HideInInspector] public Transform lerpToTransform;
	[HideInInspector] public Vector3 lerpPos, lerpRot, lerpSca;
	[HideInInspector] public EasingFunction.Enum easingFunction;
	[HideInInspector] public AnimationCurve easingCurve;
	[HideInInspector] public bool loop, useTransform = true, useLocal = true;
	[HideInInspector] public UnityEvent onFinish, onUndoFinish;

	//Private
	PositionAndScale startPos, endPos;
	float rewindSeconds = 0;
	Coroutine routine;
	Func<float, float> easingFunc;

	void Awake()
	{
		if (TryGetComponent(out BasicRewindable b))
		{
			Destroy(b);
			Debug.Log("Lerper does not need BasicRewindable script, deleting...", this);
		}
		Time.rewindListeners.Add(this);
	}

	private void OnDestroy()
	{
		Time.rewindListeners.Remove(this);
	}

	public void StartLerp()
	{
		if (routine == null)
		{
			easingFunc = easingFunction == EasingFunction.Enum.AnimationCurve ? (float x) => easingCurve.Evaluate(x) : EasingFunction.Get(easingFunction);
			if (useTransform) endPos = lerpToTransform;
			else
			{
				endPos = new PositionAndScale(lerpPos, lerpRot, lerpSca);
				if (useLocal) endPos += new PositionAndScale(transform);
			}
			if (loop)
			{
				startPos = transform;
				routine = StartCoroutine(LerpToPos(endPos, TimeToLerp, transform, () => DirectionChange(true), easingFunc));//loops dont rewind rn, probs dont need it yet
			}
			else routine = StartCoroutine(LerpToPosRewindable(endPos, TimeToLerp, () => rewindSeconds, transform, () => onFinish.Invoke(), () => onUndoFinish.Invoke(), () => routine = null, easingFunc));
		}
	}

	void DirectionChange(bool lerpBack)
	{
		if (lerpBack) routine = StartCoroutine(LerpToPos(startPos, TimeToLerp, transform, () => DirectionChange(false), easingFunc));
		else routine = StartCoroutine(LerpToPos(endPos, TimeToLerp, transform, () => DirectionChange(true), easingFunc));
	}

	private void OnEnable()
	{
		if (LerpOnEnable) StartLerp();
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		routine = null;
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

