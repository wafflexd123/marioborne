using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeText : MonoBehaviour
{
    [SerializeField] private float fadeTime = 1.0f;
    [SerializeField] private TextMeshProUGUI[] textArray;

    public void FadeInTextByWord()
    {
        StartCoroutine(FadeInWords());
    }

    public void FadeInAllText()
    {
        StartCoroutine(FadeInAll());
    }

    public void FadeOutAllText()
    {
        StartCoroutine(FadeOutAll());
    }

    private IEnumerator FadeInWords()
    {
        for (int i = 0; i < textArray.Length; i++)
        {
            while (textArray[i].color.a < 1.0f)
            {
                textArray[i].color = new Color(textArray[i].color.r, textArray[i].color.g, textArray[i].color.b, textArray[i].color.a + (Time.deltaTime / fadeTime));
                yield return null;
            }
        }
    }

    private IEnumerator FadeInAll()
    {
        bool done = false;

        while (!done)
        {
            done = true;

            for (int i = 0; i < textArray.Length; i++)
            {
                if (textArray[i].color.a < 1.0f)
                {
                    textArray[i].color = new Color(textArray[i].color.r, textArray[i].color.g, textArray[i].color.b, textArray[i].color.a + (Time.deltaTime / fadeTime));
                    done = false;
                }
            }

            yield return null;
        }
    }

    private IEnumerator FadeOutAll()
    {
        bool done = false;

        while (!done)
        {
            done = true;

            for (int i = 0; i < textArray.Length; i++)
            {
                if (textArray[i].color.a > 0.0f)
                {
                    textArray[i].color = new Color(textArray[i].color.r, textArray[i].color.g, textArray[i].color.b, textArray[i].color.a - (Time.deltaTime / fadeTime));
                    done = false;
                }
            }

            yield return null;
        }
    }
}
