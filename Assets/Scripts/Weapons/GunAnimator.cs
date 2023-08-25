using System;
using System.Collections;
using UnityEngine;

public class GunAnimator : MonoBehaviourPlus
{
	public Animation recoilAnim, idleAnim;
	Coroutine crtAnimate;
	Position startPosition;

	public void StartAnimations()
	{
		startPosition = new Position(transform, true);
		Idle();
	}

	public void StopAnimations()
	{
        if(crtAnimate != null) StopCoroutine(crtAnimate);
		crtAnimate = null;
		startPosition.ApplyToTransform(transform, true);
	}

	void Idle()
	{
		Animate(idleAnim, true);
	}

	public void Shoot()
	{
		Animate(recoilAnim, false, () => Idle());
	}

	void Animate(Animation animation, bool loop, Action onEnd = null)
	{
		ResetRoutine(E(), ref crtAnimate);
		IEnumerator E()
		{
			float timer, percent, time = animation.animateTime / 2;
			int i;
			do
			{
				Position randomPos = animation.position + startPosition;
				for (i = 0; i < 3; i++) randomPos.eulers[i] += UnityEngine.Random.Range(animation.position.eulers[i] * -animation.maxRotationVarianceMultipler, animation.position.eulers[i] * animation.maxRotationVarianceMultipler);
				for (i = 0; i < 3; i++) randomPos.coords[i] += UnityEngine.Random.Range(animation.position.coords[i] * -animation.maxPositionVarianceMultipler, animation.position.coords[i] * animation.maxPositionVarianceMultipler);

				timer = 0;
				do
				{
					timer += Time.deltaTime;
					percent = timer / time;
					transform.localEulerAngles = Vector3.Lerp(startPosition.eulers, randomPos.eulers, percent);
					transform.localPosition = Vector3.Lerp(startPosition.coords, randomPos.coords, percent);
					yield return null;
				} while (percent < 1);

				timer = 0;
				do
				{
					timer += Time.deltaTime;
					percent = timer / time;
					transform.localEulerAngles = Vector3.Lerp(randomPos.eulers, startPosition.eulers, percent);
					transform.localPosition = Vector3.Lerp(randomPos.coords, startPosition.coords, percent);
					yield return null;
				} while (percent < 1);
			} while (loop);
			crtAnimate = null;
			onEnd?.Invoke();
		}
	}

	[Serializable]
	public class Animation
	{
		public Position position;
		public float animateTime, maxPositionVarianceMultipler, maxRotationVarianceMultipler;
	}
}