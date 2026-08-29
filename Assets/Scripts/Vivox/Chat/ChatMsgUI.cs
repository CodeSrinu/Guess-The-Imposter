using TMPro;
using UnityEngine;

public class ChatMsgUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI senderName;
    [SerializeField] private TextMeshProUGUI timeStamp;
    [SerializeField] private TextMeshProUGUI messageText;

    public void SetUpMessageUI(string senderName, string timeStamp, string messageText)
    {
        this.senderName.text = senderName;
        this.timeStamp.text = timeStamp;
        this.messageText.text = messageText;
    }
}
