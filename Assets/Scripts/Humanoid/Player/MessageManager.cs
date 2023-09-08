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
    [SerializeField] private int historyLength = 5;

    private Dictionary<string, List<string>> messageHistory = new Dictionary<string, List<string>>();

    private void Start()
    {
        // Below is to test the death message function
        // DisplayRandomMessage("General Death", true);
    }

    public void DisplayRandomMessage(string listName, bool fastReveal = false)
    {
        foreach (var list in messageLists)
        {
            if (list.listName == listName)
            {
                string message = GetRandomMessage(list.messageList.messages, listName);

                if (fastReveal)
                {
                    glitchyTextScript.FastRevealText(message);
                }
                else
                {
                    glitchyTextScript.DisplayTextWithGlitch(message);
                }

                UpdateMessageHistory(listName, message);
                break;
            }
        }
    }

    private string GetRandomMessage(List<string> messages, string listName)
    {
        List<string> history = new List<string>();
        if (messageHistory.ContainsKey(listName))
        {
            history = messageHistory[listName];
        }

        List<string> availableMessages = new List<string>(messages);
        foreach (var recentMessage in history)
        {
            availableMessages.Remove(recentMessage);
        }

        if (availableMessages.Count == 0)
        {
            // All messages have been shown recently, reset available messages
            availableMessages = new List<string>(messages);
        }

        return availableMessages[Random.Range(0, availableMessages.Count)];
    }

    private void UpdateMessageHistory(string listName, string message)
    {
        if (!messageHistory.ContainsKey(listName))
        {
            messageHistory[listName] = new List<string>();
        }

        messageHistory[listName].Add(message);

        // Making sure history doesn't go past specified length
        while (messageHistory[listName].Count > historyLength)
        {
            messageHistory[listName].RemoveAt(0);
        }
    }
}
