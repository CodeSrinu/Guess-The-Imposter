using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playersJoinedTxtComp;
    [SerializeField] private TextMeshProUGUI roomCodeTxtComp;
    [SerializeField] private TextMeshProUGUI remainiingSlotsTxtComp;

    [SerializeField] private Transform joinedPlayersContainer;
    [SerializeField] private Transform annoucementContainer;


    [SerializeField] private GameObject joinedPlayerPrefab;
    [SerializeField] private GameObject announcementTxtPrefab;

    [SerializeField] private Button startBtn;
    [SerializeField] private Button leaveLobbyBtn;

    private List<string> _prevPlayerNames = new List<string>();

    private void Awake()
    {
        startBtn.onClick.AddListener(() =>
        {
            if (LobbyManager.instance.CurrentLobby.Players.Count >= 3)
            {
                GameData.playerNames.Clear();
                foreach (var player in LobbyManager.instance.CurrentLobby.Players)
                {
                    GameData.playerNames.Add(player.Data["PlayerName"].Value.Trim()
                        .Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", ""));
                }
                GameData.playersCount = LobbyManager.instance.CurrentLobby.Players.Count;

                //LoadingScreenUI.instance.StartLoading();

                NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
            }
            else
            {
                LoadingScreenUI.instance.ShowLoadingError("Need at least 3 memebers to start the game");
            }
        });

        leaveLobbyBtn.onClick.AddListener(() =>
        {
            LoadingScreenUI.instance.StartLoading();
            _ = LeaveLobbyFlow();
            LoadingScreenUI.instance.StopLoading();

        });
    }

    private void Start()
    {
        startBtn.gameObject.SetActive(NetworkManager.Singleton.IsHost);
        LobbyManager.instance.StartPolling();
        LobbyManager.instance.onLobbyUpdated += HandleLobbyChange;
    }

    private void OnDestroy()
    {
        LobbyManager.instance.onLobbyUpdated -= HandleLobbyChange;

    }

    private void HandleLobbyChange()
    {
        Lobby lobby = LobbyManager.instance.CurrentLobby;
        if (lobby == null) return;

        int playerJoined = lobby.Players.Count;
        string roomCode = lobby.LobbyCode;
        List<string> playerNames = new List<string>();


        foreach(var player in  lobby.Players)
        {
            playerNames.Add(player.Data["PlayerName"].Value.Trim());
        }

        playersJoinedTxtComp.text = "Players Joined: " + playerJoined.ToString();
        roomCodeTxtComp.text = "Room Code: "+ roomCode;
        remainiingSlotsTxtComp.text =  "Slots Remaining: " + (GameData.playersCount - playerJoined);

        DestroyJoinedPlayerNames();
        for(int i = 0; i < playerNames.Count; i++)
        {
            InstantiateJoinedPlayers(playerNames[i]);
        }

        for (int i = 0;i < (GameData.playersCount - playerJoined); i++)
        {
            InstantiateJoinedPlayers("Free Player Slot");
        }

        LoadingScreenUI.instance.StopLoading();

        foreach (string p in playerNames)
        {
            if (!_prevPlayerNames.Contains(p))
            {
                InstantiateAnnoucement(p + " joined the lobby");
            }
        }

        foreach(string p in _prevPlayerNames)
        {
            if (!playerNames.Contains(p))
            {
                InstantiateAnnoucement(p + " left the lobby");
            }
        }


        _prevPlayerNames = new List<string>(playerNames);
    }

    private void DestroyJoinedPlayerNames()
    {
        foreach(Transform child in joinedPlayersContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
    private void InstantiateJoinedPlayers(string name)
    {
        GameObject obj = Instantiate(joinedPlayerPrefab, joinedPlayersContainer);
        obj.GetComponentInChildren<TextMeshProUGUI>().text = name;
    }
    private void InstantiateAnnoucement(string annoucement)
    {

        GameObject obj = Instantiate(announcementTxtPrefab, annoucementContainer);
        obj.GetComponent<TextMeshProUGUI>().text = annoucement;
        StartCoroutine(DestroyAccouncement(obj));   
    }
    private IEnumerator DestroyAccouncement(GameObject obj)
    {
        yield return new WaitForSeconds(2);
        Destroy(obj);
    }

    private async Task LeaveLobbyFlow()
    {
        await LobbyManager.instance.LeaveLobby();
        SceneManager.LoadScene("MainMenu");
    }
}
