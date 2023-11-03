using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

public class FadeText : MonoBehaviourPlus
{
	[SerializeField] bool fadeInAll, zeroAlphasOnStart, fadeInOnStart, fadeInOnTriggerEnter, fadeOutOnTriggerExit, disableMultipleFadeIns, constraintToPlayer;
	[SerializeField] Vector3 constraintAxes;
	[SerializeField] private float fadeTime = 1.0f;
	public UnityEvent onEnter, onExit;
	[Header("Optional")]
	[SerializeField] private TextMeshProUGUI[] textArray;
	Coroutine crtFade, crtConstrain;

	private void Start()
	{
		if (textArray == null || textArray.Length == 0)
		{
			Transform transform = this.transform.GetChild(0);
			textArray = new TextMeshProUGUI[transform.childCount];
			for (int i = 0; i < textArray.Length; i++)
			{
				textArray[i] = transform.GetChild(i).GetComponent<TextMeshProUGUI>();
			}
		}
		if (zeroAlphasOnStart)
		{
			for (int i = 0; i < textArray.Length; i++)
			{
				textArray[i].alpha = 0;
			}
		}
		if (fadeInOnStart)
		{
			if (fadeInAll) FadeInAllText();
			else FadeInTextByWord();
		}
	}

	public void FadeInTextByWord()
	{
		ResetRoutine(E(), ref crtFade);
		IEnumerator E()
		{
			for (int i = 0; i < textArray.Length; i++)
			{
				while (textArray[i].alpha < 1.0f)
				{
					textArray[i].alpha += Time.deltaTime / fadeTime;
					yield return null;
				}
			}
		}
	}

	public void FadeInAllText()
	{
		ResetRoutine(E(), ref crtFade);
		IEnumerator E()
		{
			bool done = false;

			while (!done)
			{
				done = true;

				for (int i = 0; i < textArray.Length; i++)
				{
					if (textArray[i].alpha < 1.0f)
					{
						textArray[i].alpha += Time.deltaTime / fadeTime;
						done = false;
					}
				}

				yield return null;
			}
		}
	}

	public void FadeOutAllText()
	{
		ResetRoutine(E(), ref crtFade);
		IEnumerator E()
		{
			bool done = false;

			while (!done)
			{
				done = true;

				for (int i = 0; i < textArray.Length; i++)
				{
					if (textArray[i].alpha > 0.0f)
					{
						textArray[i].alpha -= Time.deltaTime / fadeTime;
						done = false;
					}
				}

				yield return null;
			}

			if (disableMultipleFadeIns) Destroy(gameObject);
		}
	}

	void ConstraintTo(Transform t)
	{
		ResetRoutine(E(), ref crtConstrain);
		IEnumerator E()
		{
			Transform child = transform.GetChild(0);
			Vector3 start = child.transform.localPosition;
			Vector3 playerStart = t.position;
			while (true)
			{
				Vector3 v = playerStart - t.position;
				v.Scale(constraintAxes);
				child.localPosition = start + child.TransformDirection(v);
				yield return null;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (fadeInOnTriggerEnter && FindComponent(other.transform, out Player p))
		{
			onEnter.Invoke();
			if (fadeInAll) FadeInAllText();
			else FadeInTextByWord();
			if (constraintToPlayer) ConstraintTo(p.transform);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (fadeOutOnTriggerExit && FindComponent(other.transform, out Player _))
		{
			onExit.Invoke();
			FadeOutAllText();
			StopCoroutine(ref crtConstrain);
		}
	}
}
