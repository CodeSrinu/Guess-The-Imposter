using UnityEngine;
using UnityEngine.UI;

public class ToggleChatPanel : MonoBehaviour
{
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private Button chatButton;
    private bool isChatVisible = false;
    private void Start()
    {
        chatButton.onClick.AddListener(OnChatBtnClicked);
    }

    private void OnChatBtnClicked()
    {
        isChatVisible = !isChatVisible;
        chatPanel.SetActive(isChatVisible);
    }
}
