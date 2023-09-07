using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    [System.Serializable]
    public class NamedMessageList
    {
        public string listName;
        public MessageListSO messageList;
    }

    [SerializeField] private TMP_Text textMesh;
    [SerializeField] private GlitchyText glitchyTextScript;
    [SerializeField] private List<NamedMessageList> messageLists = new List<NamedMessageList>();

    public void DisplayRandomMessage(string listName, bool fastReveal = false)
    {
        foreach (var list in messageLists)
        {
            if (list.listName == listName)
            {
                string message = list.messageList.messages[Random.Range(0, list.messageList.messages.Count)];

                if (fastReveal)
                {
                    glitchyTextScript.FastRevealText(message);
                }
                else
                {
                    glitchyTextScript.DisplayTextWithGlitch(message);
                }

                break;
            }
        }
    }
}
