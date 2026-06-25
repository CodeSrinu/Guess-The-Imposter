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
            playerNetworkData.name = player.name;
            playerNetworkData.hasGivenClue = player.hasGiveClue;
            playerNetworkData.hasVoted = player.hasVoted;
            playerNetworkData.isEliminated = player.isEliminated;

            Players.Add(playerNetworkData);
        }
    }

    [ClientRpc]
    public void SyncPlayerNamesToClientsClientRpc(FixedString64Bytes[] names)
    {
        List<string> playerNames = names.Select(x => x.ToString()).ToList<string>();

        GameData.playerNames = playerNames;
        PlayerManager.instance.SetUpPlayersOnly();
    }



    [ServerRpc(RequireOwnership = false)]
    public void RegisterClientServerRpc(string playerName, ulong clientId)
    {
        _privateClientIds.Add(playerName, clientId);
        _registeredClientsCount++;
        if (_registeredClientsCount >= GameData.playersCount)
        {
            FixedString64Bytes[] orderedNames = PlayerManager.instance.GetPlayers
                .Select(p => new FixedString64Bytes(p.name))
                .ToArray();
            SyncPlayerNamesToClientsClientRpc(orderedNames);
            SendPrivateDataToAll();
        }
    }

    private void SendPrivateDataToAll()
    {
        foreach (Player player in PlayerManager.instance.GetPlayers)
        {
            ClientRpcParams rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { _privateClientIds[player.name] }
                }
            };
            ReceivePrivateDataClientRpc(player.isImposter, player.assignedWord, rpcParams);
        }
        RoundManager.instance.StartWorRevealPhase();
        LobbyManager.instance.StartPolling();
        Debug.Log("SendPrivateDataToAll called");
    }

    [ClientRpc]
    private void ReceivePrivateDataClientRpc(bool _isImposter, string _assignedWord, ClientRpcParams rpcParams)
    {
        isImposter = _isImposter;
        assignedWord = _assignedWord;
    }

}
