using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
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
    private Dictionary<string, string> previousPlayersDict = new Dictionary<string, string>();
    private Dictionary<string, string> kickedPlayersDict = new Dictionary<string, string>();

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
        LobbyManager.instance.OnPlayerKickedFromLobby += HandlePlayerKickedFromLobby;
    }


    private void HandlePlayerKickedFromLobby(string playerId,string kickedPlayerName)
    {
        _annoucementManager.GiveAnnoucement($"{LobbyManager.instance.GetNameById(LobbyManager.instance.CurrentLobby.HostId)} kicked {kickedPlayerName}");
        kickedPlayersDict[playerId] = kickedPlayerName;
    }

    private void OnDestroy()
    {
        LobbyManager.instance.OnLobbyUpdated -= HandleLobbyChange;
        LobbyManager.instance.OnPlayerKickedFromLobby -= HandlePlayerKickedFromLobby;
        previousPlayersDict.Clear();
    }

    private void HandleLobbyChange()
    {
        Lobby lobby = LobbyManager.instance.CurrentLobby;

        if (lobby == null) return;

        Dictionary<string,string> currentPlayersDict = new Dictionary<string,string>();

        _lobbyPlayerUIManager.ClearAllPlayerItems();
        foreach (var player in  lobby.Players)
        {
            string name = player.Data.TryGetValue("PlayerName",out var nameData) ? nameData.Value : "Free Player Slot";
            bool isReady = false;
            bool isThisPlayerHost = player.Id == lobby.HostId;
            if(player.Data.TryGetValue("IsReady", out var readyData))
            {
                isReady = readyData.Value.ToLower() == "true";                                                  
            }
            currentPlayersDict.Add(player.Id, name);
            _lobbyPlayerUIManager.AddPlayerItem(name, isReady, isThisPlayerHost);
        }

        foreach(var playerId in currentPlayersDict.Keys)
        {
            if (!previousPlayersDict.ContainsKey(playerId))
            {
                _annoucementManager.GiveAnnoucement($"{currentPlayersDict[playerId]} is Joined the lobby");
            }
        }
        foreach(var playerId in previousPlayersDict.Keys)
        {
            if (!currentPlayersDict.ContainsKey(playerId))
            {
                if(!kickedPlayersDict.ContainsKey(playerId))
                    _annoucementManager.GiveAnnoucement($"{previousPlayersDict[playerId]} is Left the lobby");
                else
                    kickedPlayersDict.Remove(playerId);
            }
        }

        //lobby data
        int playerJoined = lobby.Players.Count;
        string roomCode = lobby.LobbyCode;
        playersJoinedTxtComp.text = "Players Joined: " + playerJoined.ToString();
        roomCodeTxtComp.text = "Room Code: "+ roomCode;
        remainiingSlotsTxtComp.text =  "Slots Remaining: " + (GameData.playersCount - playerJoined);

        for (int i = 0;i < (GameData.playersCount - playerJoined); i++)
        {
            _lobbyPlayerUIManager.AddPlayerItem("", false, false);
        }
        previousPlayersDict = new Dictionary<string, string>(currentPlayersDict);
        LoadingScreenUI.instance.StopLoading();
    }
    private async Task LeaveLobbyFlow()
    {
        await LobbyManager.instance.LeaveLobby();
        SceneManager.LoadScene("MainMenu");
    }
    
}
