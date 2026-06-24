using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    private bool _isOnline = false;
    private string _playerName;
    private Lobby _currentLobby;

    public event Action onLobbyUpdated;
    public Lobby CurrentLobby  => _currentLobby;
    public bool IsOnline
    {
        get => _isOnline;
        set => _isOnline = value;
    }


    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(GameData.playersCount);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            UnityTransport unityTransport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            unityTransport.SetRelayServerData(relayServerData);

            return joinCode;
        }
        catch(Exception e)
        {
            Debug.LogError("Relay Creation Failed: " + e.Message);
            return null;
        }
    }

    public async Task CreateLobby(string relayJoinCode, string hostName)
    {
        _playerName = hostName;
        try
        {
            Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
        {
            {"RelayJoinCode" , new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) },
            {"RoundsCount", new DataObject(DataObject.VisibilityOptions.Member, GameData.roundsCount.ToString()) },
            {"ImposterCount", new DataObject(DataObject.VisibilityOptions.Member, GameData.imposterCount.ToString()) },
            {"VotingDuration", new DataObject(DataObject.VisibilityOptions.Member, GameData.votingDuration.ToString()) },
            {"CanImposterHaveWord", new DataObject(DataObject.VisibilityOptions.Member, GameData.canImposterHaveWord.ToString()) }
        };

            Dictionary<string, PlayerDataObject> hostPlayerData = new Dictionary<string, PlayerDataObject>
        {
            {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, hostName) }
        };

            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                Data = lobbyData,
                Player = new Unity.Services.Lobbies.Models.Player
                {
                    Data = hostPlayerData
                }
            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(hostName, GameData.playersCount, lobbyOptions);

            StartHeartBeat();
        }
        catch(Exception e)
        {
            Debug.LogError("Lobby Creation Failed: " + e.Message);
        }
    }

    public async Task<bool> JoinLobby(string roomCode, string playerName)
    {
        try
        {

            _playerName = playerName;

            Dictionary<string, PlayerDataObject> clientPlayerData = new Dictionary<string, PlayerDataObject> {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            };

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = new Unity.Services.Lobbies.Models.Player { Data = clientPlayerData }
            };

            _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode, joinLobbyByCodeOptions);

            string relayJoinCode = _currentLobby.Data["RelayJoinCode"].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            UnityTransport unityTransport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            unityTransport.SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Lobby Joning failed: " + e.Message);
            return false;
        }
    }


    public async Task LeaveLobby()
    {
        try
        {
            if (NetworkManager.Singleton.IsHost)
            {
                await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
            }
            else
            {
                await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            _currentLobby = null;
            StopPolling();
            NetworkManager.Singleton.Shutdown();
        }
        catch(Exception e)
        {
            Debug.LogError("Leave lobby failed: "+ e.Message);
        }
    }

    private async void StartHeartBeat()
    {
        while (_currentLobby != null)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            await Task.Delay(15000);
        }
    }

    public async Task StartOnlineGame(string hostName)
    {
        string relayJoinCode = await CreateRelay();
        if (relayJoinCode == null) return;
        await CreateLobby(relayJoinCode, hostName);
        NetworkManager.Singleton.StartHost();
    }

    public async Task PollLobby()
    {
        while(_currentLobby != null)
        {
            await Task.Delay(1500);
            _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
            onLobbyUpdated?.Invoke();
        }
    }

    public void StartPolling()
    {
        _ = PollLobby();
    }

    public void StopPolling()
    {
        _currentLobby = null;
    }
}
