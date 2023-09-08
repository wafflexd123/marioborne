using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MessageList", menuName = "Messages/MessageList", order = 1)]
public class MessageListSO : ScriptableObject
{
    [TextArea(3, 10)]
    public List<string> messages = new List<string>();
}
