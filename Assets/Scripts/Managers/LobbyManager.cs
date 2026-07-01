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

    public async Task<(string, string)> CreateRelay()
    {
        string errorMsg = "";
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(GameData.playersCount);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            UnityTransport unityTransport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            unityTransport.SetRelayServerData(relayServerData);

            errorMsg = "";
            return (joinCode, errorMsg);
        }
        catch(RelayServiceException e)
        {
            Debug.LogError("Relay Creation Failed: " + e.Message);

            switch (e.Reason)
            {
                case RelayExceptionReason.RateLimited:
                    errorMsg = "Too many Attempts. Please wait a moment and try again.";
                    break;
                case RelayExceptionReason.Forbidden:
                    errorMsg = "You don't have permission to do that right now.";
                    break;
                case RelayExceptionReason.EntityNotFound:
                    errorMsg = "The game session doesn't exist anymore.";
                    break;
                case RelayExceptionReason.NetworkError:
                    errorMsg = "Connection problem. Check your connection and try again.";
                    break;
                case RelayExceptionReason.Unknown:
                    errorMsg = "Something went wrong, Please try again.";
                    break;
                default:
                    errorMsg = "Something went wrong, Please try again.";
                    break;
            }


            return (null, errorMsg);
        }
    }

    public async Task<string> CreateLobby(string relayJoinCode, string hostName)
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
            {"CanImposterHaveWord", new DataObject(DataObject.VisibilityOptions.Member, GameData.canImposterHaveWord.ToString())},
            {"PlayersCount", new DataObject(DataObject.VisibilityOptions.Member, GameData.playersCount.ToString()) } 
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
            
            return null;
        }
        catch(LobbyServiceException e)
        {
            Debug.LogError("Lobby Creation Failed: " + e.Message);
            string errorMsg = "";

            switch (e.Reason)
            {
                case LobbyExceptionReason.RateLimited:
                    errorMsg = "Too many Attempts. Please wait a moment and try again.";
                    break;
                case LobbyExceptionReason.LobbyNotFound:
                    errorMsg = "That room doesn't exist. Check the code and try again.";
                    break;
                case LobbyExceptionReason.LobbyFull:
                    errorMsg = "That room is already full.";
                    break;
                case LobbyExceptionReason.LobbyAlreadyExists:
                    errorMsg = "A room with that code already exists.";
                    break;
                case LobbyExceptionReason.Forbidden:
                    errorMsg = "You don't have permission to do that right now.";
                    break;
                default:
                    errorMsg = "Something went wrong, Please try again.";
                    break;
            }

            return errorMsg;
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
            NetworkManager.Singleton.OnClientConnectedCallback += (id) => {
                Debug.Log("Client connected: " + id);
            };

            NetworkManager.Singleton.StartClient();
            Debug.Log("StartClient called, IsClient: " + NetworkManager.Singleton.IsClient);

            GameData.roundsCount = int.Parse(_currentLobby.Data["RoundsCount"].Value);
            GameData.imposterCount = int.Parse(_currentLobby.Data["ImposterCount"].Value);
            GameData.votingDuration = float.Parse(_currentLobby.Data["VotingDuration"].Value);
            GameData.canImposterHaveWord = bool.Parse(_currentLobby.Data["CanImposterHaveWord"].Value);
            GameData.playersCount = int.Parse(_currentLobby.Data["PlayersCount"].Value);


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

    public async Task<bool> StartOnlineGame(string hostName)
    {
        var (code, error) = await CreateRelay();
        string relayJoinCode = code;
        string relayErrorMsg = error;
        if (relayJoinCode == null)
        {
            LoadingScreenUI.instance.ShowLoadingError(relayErrorMsg);
            return false;
        }
        string lobbyErrorMsg = await CreateLobby(relayJoinCode, hostName);
        if(lobbyErrorMsg != null)
        {
            LoadingScreenUI.instance.ShowLoadingError(lobbyErrorMsg);
            return false;
        }

        NetworkManager.Singleton.StartHost();
        return true;
    }

    public async Task PollLobby()
    {
        Debug.Log("Inside Poll Lobby, curent lobby = " +  _currentLobby);
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
