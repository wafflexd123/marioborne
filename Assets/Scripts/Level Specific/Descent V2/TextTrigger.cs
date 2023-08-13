using UnityEngine;

public class TextTrigger : MonoBehaviour
{
    [SerializeField] private FadeText fadeText;
    [SerializeField] private bool fadeInAll = false;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !fadeInAll)
        {
            // Change this to FadeInEntireText() or FadeInTextByWord() depending on what you want
            fadeText.FadeInTextByWord();
        }
        else
        {
            fadeText.FadeInAllText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            fadeText.FadeOutAllText();
        }
    }
}
