using System.Collections;
using TMPro;
using UnityEngine;

public class GlitchyText : MonoBehaviour
{
    [SerializeField] private TMP_Text textMesh;
    [SerializeField] private float timePerCharacter = 0.5f;
    [SerializeField] private float fastRevealTime = 1.0f;
    private const char PLACEHOLDER = '_';

    private float glitchDuration;
    private int maxGlitchIterations;

    private void Awake()
    {
        glitchDuration = timePerCharacter / 10f;
        maxGlitchIterations = Mathf.Max(1, (int)(timePerCharacter / glitchDuration));
    }

    public void DisplayTextWithGlitch(string message)
    {
        gameObject.SetActive(true);
        StartCoroutine(GlitchyTextRoutine(message));
    }

    public void FastRevealText(string message)
    {
        gameObject.SetActive(true);
        StartCoroutine(FastRevealRoutine(message));
    }

    private IEnumerator GlitchyTextRoutine(string message)
    {
        textMesh.text = "";

        for (int i = 0; i < message.Length; i++)
        {
            // Skip to end of tags if they're found
            if (message[i] == '<')
            {
                int tagEnd = message.IndexOf('>', i);
                if (tagEnd >= 0)
                {
                    // Append tags to displayed text
                    textMesh.text += message.Substring(i, tagEnd - i + 1);
                    i = tagEnd;
                    continue;
                }
            }

            textMesh.text += PLACEHOLDER;

            for (int j = 0; j < maxGlitchIterations; j++)
            {
                char randomChar = (char)Random.Range(33, 127);
                textMesh.text = textMesh.text.Substring(0, i) + randomChar;
                yield return new WaitForSeconds(glitchDuration);
            }

            textMesh.text = textMesh.text.Substring(0, i) + message[i];
        }
    }

    private IEnumerator FastRevealRoutine(string message)
    {
        textMesh.text = "";

        for (int i = 0; i < message.Length; i++)
        {
            // Skip to end of tags if they're found
            if (message[i] == '<')
            {
                int tagEnd = message.IndexOf('>', i);
                if (tagEnd >= 0)
                {
                    // Append tags to displayed text
                    textMesh.text += message.Substring(i, tagEnd - i + 1);
                    i = tagEnd;
                    continue;
                }
            }

            textMesh.text += message[i];
            yield return new WaitForSeconds(fastRevealTime / message.Length);
        }
    }

}
