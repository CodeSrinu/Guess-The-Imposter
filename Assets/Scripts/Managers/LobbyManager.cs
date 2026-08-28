using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.LowLevelPhysics2D.PhysicsLayers;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager instance;
    private bool _isOnline = false;

    private Lobby _currentLobby;

    public event Action OnLobbyUpdated;
    public event Action<string, string> OnPlayerKickedFromLobby;

    public Lobby CurrentLobby  => _currentLobby;
    private bool _isPolling = false;
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
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, hostName) },
                {"IsReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "true") }
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
            await ChatManager.Instance.JoinChannel(relayJoinCode);
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
        bool isJoined = false;
        try
        {
            Dictionary<string, PlayerDataObject> clientPlayerData = new Dictionary<string, PlayerDataObject> {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            };

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = new Unity.Services.Lobbies.Models.Player { Data = clientPlayerData }
            };

            _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode, joinLobbyByCodeOptions);
            isJoined = true;
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
            await ChatManager.Instance.JoinChannel(relayJoinCode);
            return true;
        }
        catch (Exception e)
        {
            if (isJoined)
            {
                await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId);
                LoadingScreenUI.instance.ShowLoadingError("Network Error: Lobby joining failed, try again");
                _currentLobby = null;
            }
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
            await ChatManager.Instance.LeaveChannel();
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        catch(Exception e)
        {
            Debug.LogError("Leave lobby failed: "+ e.Message);
            LoadingScreenUI.instance.ShowLoadingError("Exit Lobby failed, Try again");
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
        while(_currentLobby != null && _isPolling)
        {
            try
            {
                await Task.Delay(1500);
                _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
                OnLobbyUpdated?.Invoke();
            }
            catch(LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.Forbidden)
                {
                    //LoadingScreenUI.instance.ShowLoadingError($"You are kicked by the Host");
                }
                else if (e.Reason == LobbyExceptionReason.LobbyNotFound && !NetworkManager.Singleton.IsHost)
                {
                    LoadingScreenUI.instance.ShowLoadingError("Host deleted the lobby");
                }
                else if(e.Reason == LobbyExceptionReason.Unknown)
                {
                    LoadingScreenUI.instance.ShowLoadingError("Something Went Wrong");
                    return;
                }
                else
                {
                    LoadingScreenUI.instance.ShowLoadingError($"Network Error: Lobby Not Found");
                }
                StopPolling();
                _currentLobby = null;

                //await Task.Delay(2000);
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    public void StartPolling()
    {
        _isPolling = true;
        _ = PollLobby();
    }

    public void StopPolling()
    {
       _isPolling = false;
    }


    public void SetPlayerReadyStatus(bool ready)
    {
        UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
        {
            Data = new Dictionary<string, PlayerDataObject>()
            {
                { "IsReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ready.ToString())}
            }
        };

        LobbyService.Instance.UpdatePlayerAsync(
            _currentLobby.Id,
            AuthenticationService.Instance.PlayerId,
            updatePlayerOptions
        );
        OnLobbyUpdated?.Invoke();
    }

    public void ResetPlayerReadyStatus()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        NetworkPlayerManager.instance.ForceResetReadyStatusCLientRpc();
    }



    public void KickPlayer(string playerName)
    {
        foreach(var player in _currentLobby.Players)
        {
            if (player.Data.TryGetValue("PlayerName", out var nameData) && nameData.Value == playerName)
            {
                LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, player.Id);
                BrodcastPlayerKickedEventClientRpc(player.Id, playerName);
                OnLobbyUpdated?.Invoke();
                return;
            }
        }

    }


    [ClientRpc]
    public void BrodcastPlayerKickedEventClientRpc(string playerId,string playerName)
    {
        OnPlayerKickedFromLobby?.Invoke(playerId, playerName);
        if(playerId == AuthenticationService.Instance.PlayerId)
        {
            LoadingScreenUI.instance.ShowLoadingError($"You are kicked by the host:{GetNameById(_currentLobby.HostId)}");
            StopPolling();
            _currentLobby = null;
            SceneManager.LoadScene("MainMenu");
        }
    }

    public string GetNameById(string id)
    {
        foreach (var player in _currentLobby.Players)
        {
            if (player.Id == id) return player.Data["PlayerName"].Value;
        }
        return null;
    }
}
