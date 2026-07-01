using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerManager : NetworkBehaviour
{
    public NetworkList<PlayerNetworkData> Players;

    public bool isImposter = false;
    public string assignedWord = "";


    private Dictionary<string, ulong> _privateClientIds = new Dictionary<string, ulong>();
    private int _registeredClientsCount;
    private int _confirmedClientCount = 0;
    public static NetworkPlayerManager instance;

    private void Awake()
    {
        Players = new NetworkList<PlayerNetworkData>();


        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

    }

    public void PopulatePlayers()
    {
        if (!IsHost) return;

        foreach(Player player in PlayerManager.instance.GetPlayers)
        {
            PlayerNetworkData playerNetworkData = new PlayerNetworkData();
            playerNetworkData.name = player.name.Trim();
            playerNetworkData.hasGivenClue = player.hasGiveClue;
            playerNetworkData.hasVoted = player.hasVoted;
            playerNetworkData.isEliminated = player.isEliminated;

            Players.Add(playerNetworkData);
        }
    }

    [ClientRpc]
    public void SyncPlayerNamesToClientsClientRpc(FixedString64Bytes[] names)
    {
        if(IsHost) return;

        Debug.Log("SyncPlayerNamesToClientsClientRpc received, names: " + string.Join(", ", names));

        List<string> playerNames = names.Select(x => x.ToString()).ToList<string>();

        GameData.playerNames = playerNames;
        PlayerManager.instance.SetUpPlayersOnly();
    }



    [ServerRpc(RequireOwnership = false)]
    public void RegisterClientServerRpc(string playerName, ulong clientId)
    {
        Debug.Log("RegisterClientServerRpc called, name: " + playerName + " count now: " + (_registeredClientsCount + 1) + " needed: " + (GameData.playerNames.Count - 1));
        _privateClientIds.Add(playerName, clientId);
        _registeredClientsCount++;
        if (_registeredClientsCount >= GameData.playerNames.Count - 1)
        {
            FixedString64Bytes[] orderedNames = PlayerManager.instance.GetPlayers
                .Select(p => new FixedString64Bytes(p.name))
                .ToArray();
            Debug.Log("Calling SyncPlayerNamesToClientsClientRpc with: " + string.Join(", ", orderedNames));
            SyncPlayerNamesToClientsClientRpc(orderedNames);
            SendPrivateDataToAll();
        }
    }

    [ClientRpc]
    public void StartTimerClientRpc(float duration)
    {
        if (IsHost) return;
        Timer.instance.StartTimer(duration, null);
    }

    [ClientRpc]
    public void StopTimerClientRpc()
    {
        if (IsHost) return;
        Timer.instance.StopTimer();
    }

    private void SendPrivateDataToAll()
    {
        

        foreach (Player player in PlayerManager.instance.GetPlayers)
        {
            if (!_privateClientIds.ContainsKey(player.name)) continue;


            ClientRpcParams rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { _privateClientIds[player.name] }
                }
            };
            Debug.Log("Sending to " + player.name + " isImposter: " + player.isImposter + " word: " + player.assignedWord);
            ReceivePrivateDataClientRpc(player.isImposter, player.assignedWord, rpcParams);
        }

        Player hostPlayer = PlayerManager.instance.GetPlayers.Find(p => !_privateClientIds.ContainsKey(p.name));
        if (hostPlayer != null)
        {
            isImposter = hostPlayer.isImposter;
            assignedWord = hostPlayer.assignedWord;
            GameData.devicePlayerName = hostPlayer.name;
            LoadingScreenUI.instance.StopLoading();
        }
        Debug.Log("SendPrivateDataToAll, player count: " + PlayerManager.instance.GetPlayers.Count);
        Debug.Log("Host data before phase: isImposter=" + isImposter + " word=" + assignedWord);
    }


    [ServerRpc(RequireOwnership = false)]
    private void ConfirmReceivedWordServerRpc()
    {
        _confirmedClientCount++;
        int totalClientsCount = GameData.playerNames.Count - 1;
        if(_confirmedClientCount == totalClientsCount)
        {
            _confirmedClientCount = 0;
            RoundManager.instance.StartWordRevealPhase();
            Timer.instance.StartTimer(10f, RoundManager.instance.StartCluePhase);
            StartTimerClientRpc(10f);
        }
    }


    [ClientRpc]
    private void ReceivePrivateDataClientRpc(bool _isImposter, string _assignedWord, ClientRpcParams rpcParams)
    {
        isImposter = _isImposter;
        assignedWord = _assignedWord;
        LoadingScreenUI.instance.StopLoading();
        UIManager.instance.SetUpWordRevealPanel();
        ConfirmReceivedWordServerRpc();
    }

    public void ResetRegistrationState()
    {
        _privateClientIds.Clear();
        _registeredClientsCount = 0;
        _confirmedClientCount = 0;
        Players.Clear();
    }

    public bool IsPlayerEliminated(string playerName)
    {
        foreach(PlayerNetworkData player in Players)
        {
            if (player.name.ToString() == playerName) return player.isEliminated;
        }
        return false;
    }

}
