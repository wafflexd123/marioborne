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

    [SerializeField] private GlitchyText glitchyTextMain;
    [SerializeField] private GlitchyText glitchyTextRestart;
    [SerializeField] private List<NamedMessageList> messageLists = new List<NamedMessageList>();
    [SerializeField] private int historyLength = 5;

    private Dictionary<string, List<string>> messageHistory = new Dictionary<string, List<string>>();

    public void DisplayRandomMessage(DeathType deathType, bool fastReveal = false)
    {
        string listName = "";
        float generalDeathProbability = 0.5f; // 50% chance to show general message

        // Decide if we should show a general death message
        if (Random.value < generalDeathProbability)
        {
            listName = "General Death";
        }
        else
        {
            switch (deathType)
            {
                case DeathType.General:
                    listName = "General Death";
                    break;
                case DeathType.Bullet:
                    listName = "Bullet Death";
                    break;
                case DeathType.Fall:
                    listName = "Fall Death";
                    break;
                case DeathType.Melee:
                    listName = "Melee Death";
                    break;
            }
        }

        NamedMessageList list = messageLists.Find(l => l.listName == listName);
        if (list == null) return;

        string message = GetRandomMessage(list.messageList.messages, listName);
        if (fastReveal)
        {
            glitchyTextMain.FastRevealText(message);
            if (deathType != DeathType.General)
                ShowRestartKey();
        }
        else
        {
            glitchyTextMain.DisplayTextWithGlitch(message);
            if (deathType != DeathType.General)
                ShowRestartKey();
        }
        UpdateMessageHistory(listName, message);
    }

    public void Hide()
	{
        glitchyTextMain.gameObject.SetActive(false);
        glitchyTextRestart.gameObject.SetActive(false);
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

    private void ShowRestartKey()
    {
        glitchyTextRestart.FastRevealText("Hold R to Rewind");
    }
}
