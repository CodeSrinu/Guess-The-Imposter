using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
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
    [SerializeField] private Button readyBtn;
    [SerializeField] private AnnoucementManager _annoucementManager;
    [SerializeField] private LobbyPlayersUIManager _lobbyPlayerUIManager;

    private bool isReady = false;

    private void Awake()
    {
        readyBtn.gameObject.SetActive(!NetworkManager.Singleton.IsHost);

        startBtn.onClick.AddListener(() =>
        {
            CheckAllAreReadyAndStart();
        });

        leaveLobbyBtn.onClick.AddListener(() =>
        {
            LoadingScreenUI.instance.StartLoading();
            _ = LeaveLobbyFlow();
            LoadingScreenUI.instance.StopLoading();

        });

        readyBtn.onClick.AddListener(() =>
        {
            isReady = !isReady;
            LobbyManager.instance.SetPlayerReadyStatus(isReady);
            readyBtn.GetComponentInChildren<TextMeshProUGUI>().text = isReady ? "Cancel" : "Ready";
        });
        
    }


    private void CheckAllAreReadyAndStart()
    {
        if (LobbyManager.instance.CurrentLobby.Players.Count < 3)
        {
            LoadingScreenUI.instance.ShowLoadingError("Need at least 3 players to start the game");
            return;
        }

        int readyCount = 0;
        foreach(var player in LobbyManager.instance.CurrentLobby.Players)
        {
            if (player.Data.TryGetValue("IsReady", out var readyData) && readyData.Value.ToLower() == "true")
            {
                readyCount++;
            }
        }
        
        if(readyCount < LobbyManager.instance.CurrentLobby.Players.Count) 
        { 
            LoadingScreenUI.instance.ShowLoadingError("Not All players are Ready");
            return;
        }

        GameData.playerNames.Clear();
        foreach (var player in LobbyManager.instance.CurrentLobby.Players)
        {
            GameData.playerNames.Add(player.Data["PlayerName"].Value.Trim()
                .Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", ""));
        }
        GameData.playersCount = LobbyManager.instance.CurrentLobby.Players.Count;

        LoadingScreenUI.instance.StartLoading();
        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);

    }

    private void Start()
    {
        startBtn.gameObject.SetActive(NetworkManager.Singleton.IsHost);
        LobbyManager.instance.StartPolling();
        LobbyManager.instance.ResetPlayerReadyStatus();
        LobbyManager.instance.OnLobbyUpdated += HandleLobbyChange;
        LobbyManager.instance.OnLobbyCreation += HandleLobbyCreation;
        LobbyManager.instance.OnPlayerJoinedLobby += HandlePlayerJoinedLobby;
        LobbyManager.instance.OnPlayerKickedFromLobby += HandlePlayerKickedFromLobby;
        LobbyManager.instance.OnPlayerLeftLobby += HandlePlayerLeftLobby;

        //_annoucementManager = FindAnyObjectByType<AnnoucementManager>();
        //_lobbyPlayerUIManager = FindAnyObjectByType<LobbyPlayersUIManager>();

        
    }

    private void HandlePlayerLeftLobby(string playerName)
    {
        _annoucementManager.GiveAnnoucement($"{playerName} left the lobby");
    }

    private void HandlePlayerKickedFromLobby(string kickedPlayerName)
    {
        _annoucementManager.GiveAnnoucement($"Host kicked {kickedPlayerName}");
    }

    private void HandlePlayerJoinedLobby(string joinedPlayerName)
    {
        _annoucementManager.GiveAnnoucement($"{joinedPlayerName} joined the lobby");
        _lobbyPlayerUIManager.AddPlayerItem(joinedPlayerName,false,false);
    }

    private void HandleLobbyCreation()
    {
        _annoucementManager.GiveAnnoucement("New Lobby Created");
    }

    private void OnDestroy()
    {
        LobbyManager.instance.OnLobbyUpdated -= HandleLobbyChange;
        LobbyManager.instance.OnLobbyCreation -= HandleLobbyCreation;
        LobbyManager.instance.OnPlayerJoinedLobby -= HandlePlayerJoinedLobby;
        LobbyManager.instance.OnPlayerKickedFromLobby -= HandlePlayerKickedFromLobby;
        LobbyManager.instance.OnPlayerLeftLobby -= HandlePlayerLeftLobby;
    }

    private void HandleLobbyChange()
    {
        Lobby lobby = LobbyManager.instance.CurrentLobby;

        if (lobby == null) return;

        int playerJoined = lobby.Players.Count;
        string roomCode = lobby.LobbyCode;
        List<string> playerNames = new List<string>();

        //DestroyJoinedPlayerNames();
        foreach (var player in  lobby.Players)
        {
            string name = player.Data.TryGetValue("PlayerName",out var nameData) ? nameData.Value : "Free Player Slot";
            bool isReady = false;
            bool isThisPlayerHost = player.Id == lobby.HostId;
            if(player.Data.TryGetValue("IsReady", out var readyData))
            {
                isReady = readyData.Value.ToLower() == "true";                                                  
            }

            playerNames.Add(name);
            //InstantiateJoinedPlayers(name, isReady, isThisPlayerHost);
            _lobbyPlayerUIManager.AddPlayerItem(name, isReady, isThisPlayerHost);
        }

        playersJoinedTxtComp.text = "Players Joined: " + playerJoined.ToString();
        roomCodeTxtComp.text = "Room Code: "+ roomCode;
        remainiingSlotsTxtComp.text =  "Slots Remaining: " + (GameData.playersCount - playerJoined);

        for (int i = 0;i < (GameData.playersCount - playerJoined); i++)
        {
            //InstantiateJoinedPlayers("");
            _lobbyPlayerUIManager.AddPlayerItem("", false, false);
        }

        LoadingScreenUI.instance.StopLoading();
    }

    //private void DestroyJoinedPlayerNames()
    //{
    //    foreach(Transform child in joinedPlayersContainer.transform)
    //    {
    //        Destroy(child.gameObject);
    //    }
    //}
    //private void InstantiateJoinedPlayers(string name, bool isReady = false, bool isThisPlayerHost = false)
    //{
    //    GameObject item = Instantiate(joinedPlayerPrefab, joinedPlayersContainer);
        
    //}
    //private void InstantiateAnnoucement(string annoucement)
    //{
    //    GameObject announcement = Instantiate(announcementTxtPrefab, annoucementContainer);
    //    announcement.GetComponent<TextMeshProUGUI>().text = annoucement;
    //    StartCoroutine(DestroyAccouncement(announcement));   
    //}
    //private IEnumerator DestroyAccouncement(GameObject obj)
    //{
    //    yield return new WaitForSeconds(2);
    //    Destroy(obj);
    //}


    private async Task LeaveLobbyFlow()
    {
        await LobbyManager.instance.LeaveLobby();
        SceneManager.LoadScene("MainMenu");
    }
}
