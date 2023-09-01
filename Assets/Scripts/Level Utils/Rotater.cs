using System.Collections;
using UnityEngine;

public class Rotater : MonoBehaviour
{
	public EasingFunction.Enum easingFunction;
	public Vector3 rotateAmount;
	[field: SerializeField] public float TimeToRotate { get; set; }
	[field: SerializeField] public bool RotateOnEnable { get; set; }
	[field: SerializeField] public bool Loop { get; set; }

	public void StartRotation()
	{
		Vector3 start = transform.eulerAngles;
		Vector3 end = rotateAmount + start;
		float timer, percent;
		StartCoroutine(Rotate());
		IEnumerator Rotate()
		{
			do
			{
				timer = 0;
				do
				{
					timer += Time.fixedDeltaTime;
					percent = timer / TimeToRotate;
					transform.eulerAngles = Vector3.Lerp(start, end, EasingFunction.Get(easingFunction)(percent));
					yield return new WaitForFixedUpdate();
				} while (percent < 1);
			} while (Loop);
		}
	}

	private void OnEnable()
	{
		if (RotateOnEnable) StartRotation();
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}
}
