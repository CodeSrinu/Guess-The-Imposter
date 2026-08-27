using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTxtComp;
    [SerializeField] private TextMeshProUGUI isReadyTxtComp;
    [SerializeField] private Button kickBtn;
    
    public void Initialize(bool isThisPlayerHost, string name = "")
    {
        SetPlayer(name, isThisPlayerHost);
        this.isReadyTxtComp.gameObject.SetActive(false);
    }

    private void SetPlayer(string playerName, bool isThisPlayerItemHost = false)
    {
        bool canKick = NetworkManager.Singleton.IsHost && !isThisPlayerItemHost && playerName != "";
        nameTxtComp.text = playerName == "" ? "Free Player Slot": playerName;
        kickBtn.gameObject.SetActive(canKick);
        kickBtn.onClick.RemoveAllListeners();
        kickBtn.onClick.AddListener(() =>
        {
            LobbyManager.instance.KickPlayer(playerName);
        });
    }

    public void SetPlayerReadyStatus(bool isReady)
    {
        isReadyTxtComp.text = isReady ? "Ready":"Not Ready";
        isReadyTxtComp.color = isReady ? Color.green:Color.red;
        isReadyTxtComp.gameObject.SetActive(true);
    }
}
