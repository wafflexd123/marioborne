using System;
using UnityEngine;
using UnityEngine.Events;

public class Lerper : UnityEventHelper, IRewindListener
{
	public enum LoopType { None, PingPong, Teleport }
	[field: SerializeField] public float TimeToLerp { get; set; }
	[field: SerializeField] public bool LerpOnEnable { get; set; }

	//These are handled by the custom inspector
	[HideInInspector] public Transform lerpToTransform;
	[HideInInspector] public Vector3 lerpPos, lerpRot, lerpSca;
	[HideInInspector] public EasingFunction.Enum easingFunction;
	[HideInInspector] public AnimationCurve easingCurve;
	[HideInInspector] public LoopType loopType;
	[HideInInspector] public bool useTransform = true, useLocal = true;
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
			startPos = transform;
			easingFunc = easingFunction == EasingFunction.Enum.AnimationCurve ? (float x) => easingCurve.Evaluate(x) : EasingFunction.Get(easingFunction);

			if (useTransform)
			{
				endPos = lerpToTransform;
			}
			else
			{
				endPos = new PositionAndScale(lerpPos, lerpRot, lerpSca);
				if (useLocal) endPos += new PositionAndScale(transform);
			}

			switch (loopType)
			{
				case LoopType.None:
					routine = StartCoroutine(LerpToPosRewindable(endPos, TimeToLerp, () => rewindSeconds, transform, () => onFinish.Invoke(), () => onUndoFinish.Invoke(), () => routine = null, easingFunc));
					break;
				case LoopType.PingPong:
					PingPong(false);//loops dont rewind rn, probs dont need it yet
					break;
				case LoopType.Teleport:
					Teleport();//loops dont rewind rn, probs dont need it yet
					break;
			}
		}
	}

	void Teleport()
	{
		routine = StartCoroutine(LerpToPos(endPos, TimeToLerp, transform, () => { transform.position = startPos; Teleport(); }, easingFunc));
	}

	void PingPong(bool lerpBack)
	{
		if (lerpBack) routine = StartCoroutine(LerpToPos(startPos, TimeToLerp, transform, () => PingPong(false), easingFunc));
		else routine = StartCoroutine(LerpToPos(endPos, TimeToLerp, transform, () => PingPong(true), easingFunc));
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

