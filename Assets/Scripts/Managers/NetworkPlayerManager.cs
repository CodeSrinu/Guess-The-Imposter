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
    public void StartTimerClientRpc()
    {
        Timer.instance.StartTimer(10f, null);
    }

    private void SendPrivateDataToAll()
    {
        string hostName = GameData.playerNames[0];
        Player hostPlayer = PlayerManager.instance.GetPlayers.Find(p => p.name == hostName);
        isImposter = hostPlayer.isImposter;
        assignedWord = hostPlayer.assignedWord;

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
        Debug.Log("SendPrivateDataToAll, player count: " + PlayerManager.instance.GetPlayers.Count);
        Debug.Log("Host data before phase: isImposter=" + isImposter + " word=" + assignedWord);
        RoundManager.instance.StartWordRevealPhase();


    }

    [ClientRpc]
    private void ReceivePrivateDataClientRpc(bool _isImposter, string _assignedWord, ClientRpcParams rpcParams)
    {
        isImposter = _isImposter;
        assignedWord = _assignedWord;
        UIManager.instance.SetUpWordRevealPanel();
    }


    
}
