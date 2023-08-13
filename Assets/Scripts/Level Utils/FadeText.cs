using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeText : MonoBehaviourPlus
{
	[SerializeField] bool fadeInAll, zeroAlphasOnStart, fadeInOnStart, fadeOutOnTriggerExit, disableMultipleFadeIns;
	[SerializeField] private float fadeTime = 1.0f;
	[Header("Optional")]
	[SerializeField] private TextMeshProUGUI[] textArray;
	Coroutine crtFade;

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

	private void OnTriggerEnter(Collider other)
	{
		if (FindComponent(other.transform, out Player _))
		{
			if (fadeInAll) FadeInAllText();
			else FadeInTextByWord();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (fadeOutOnTriggerExit && FindComponent(other.transform, out Player _))
		{
			FadeOutAllText();
		}
	}
}
