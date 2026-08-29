using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChatPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField messageInputFeild;
    [SerializeField] private Button sendBtn;
    [SerializeField] private Button chatButton;
    [SerializeField] private Button closeChatBtn;
    [SerializeField] private Transform messagesContainer;
    [SerializeField] private GameObject msesagePrefab;


    private void Start()
    {
        sendBtn.onClick.AddListener(OnSendClicked);
        chatButton.onClick.AddListener(() => { gameObject.SetActive(true);  });
        closeChatBtn.onClick.AddListener(() => { gameObject.SetActive(false); });
        ChatManager.Instance.OnMessageReceived += HandleMessageReceived;
        gameObject.SetActive(false);
        chatButton.gameObject.SetActive(false);
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        if(scene.name == "Lobby") chatButton.gameObject.SetActive(true);
    }

    private void HandleMessageReceived(ChatMessage msg)
    {
        AddMsgToUI(msg);
    }

    private async void OnSendClicked()
    {
        if (string.IsNullOrEmpty(messageInputFeild.text)) return;

         await ChatManager.Instance.SendTextMessage(messageInputFeild.text);
        messageInputFeild.text = "";
    }

    private void AddMsgToUI(ChatMessage msg)
    {
        GameObject uiMsgObj = Instantiate(msesagePrefab, messagesContainer);
        uiMsgObj.GetComponent<ChatMsgUI>().SetUpMessageUI(msg.SenderName, msg.Timestamp, msg.Text);
    }
}
