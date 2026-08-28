using UnityEngine;

public class ChatMessage 
{
    public string SenderName { get; }
    public string Text { get; }
    public string Timestamp { get; }

    public ChatMessage(string senderName, string text, string timestamp)
    {
        SenderName = senderName;
        Text = text;
        Timestamp = timestamp;
    }
}
