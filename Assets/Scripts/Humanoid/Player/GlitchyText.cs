using System.Collections;
using TMPro;
using UnityEngine;

public class GlitchyText : MonoBehaviour
{
    [SerializeField] private TMP_Text textMesh;
    [SerializeField] private float totalGlitchTime = 1.0f; // Total time for the glitch effect to complete
    [SerializeField] private float fastRevealTime = 1.0f;
    private const char PLACEHOLDER = '_';

    private Coroutine currentCoroutine; // Reference to the current running coroutine

    public void DisplayTextWithGlitch(string message)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        gameObject.SetActive(true);
        currentCoroutine = StartCoroutine(GlitchyTextRoutine(message));
    }

    public void FastRevealText(string message)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        gameObject.SetActive(true);
        currentCoroutine = StartCoroutine(FastRevealRoutine(message));
    }

    private IEnumerator GlitchyTextRoutine(string message)
    {
        textMesh.text = "";
        int totalGlitches = message.Length * 10; // For example, 10 glitches per character
        float glitchDuration = totalGlitchTime / totalGlitches;

        int glitchCount = 0;

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

            // Placeholder character
            textMesh.text += PLACEHOLDER;

            // Perform glitches
            for (int j = 0; j < 10; j++) // Glitch 10 times per character
            {
                char randomChar = (char)Random.Range(33, 127);
                textMesh.text = textMesh.text.Substring(0, i) + randomChar + textMesh.text.Substring(i + 1);
                glitchCount++;

                // Wait for glitch duration
                if (glitchCount < totalGlitches)
                {
                    yield return new WaitForSecondsRealtime(glitchDuration);
                }
            }

            // Reveal the actual character
            textMesh.text = textMesh.text.Substring(0, i) + message[i] + textMesh.text.Substring(i + 1);
        }

        currentCoroutine = null; // Reset the coroutine reference when done
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
            yield return new WaitForSecondsRealtime(fastRevealTime / message.Length);
        }

        currentCoroutine = null; // Reset the coroutine reference when done
    }
}
