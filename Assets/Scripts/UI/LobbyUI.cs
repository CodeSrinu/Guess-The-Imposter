using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private Transform joinedPlayersContainer;
    [SerializeField] private Transform annoucementContainer;


    [SerializeField] private GameObject joinedPlayerPrefab;
    [SerializeField] private GameObject announcementTxtPrefab;

    [SerializeField] private Button startBtn;

    private List<string> _prevPlayerNames = new List<string>();

    private void Awake()
    {
        startBtn.onClick.AddListener(() =>
        {
            if (LobbyManager.instance.CurrentLobby.Players.Count >= 3)
            {
                LobbyManager.instance.StopPolling();
                NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
            }
        });
    }

    private void Start()
    {
        startBtn.gameObject.SetActive(NetworkManager.Singleton.IsHost);

        LobbyManager.instance.onLobbyUpdated += HandleLobbyChange;
        LobbyManager.instance.StartPolling();
    }

    private void HandleLobbyChange()
    {
        Lobby lobby = LobbyManager.instance.CurrentLobby;

        int playerJoined = lobby.Players.Count;
        string roomCode = lobby.LobbyCode;
        List<string> playerNames = new List<string>();


        foreach(var player in  lobby.Players)
        {
            playerNames.Add(player.Data["PlayerName"].Value);
        }

        playersJoinedTxtComp.text = playerJoined.ToString();
        roomCodeTxtComp.text = roomCode;

        DestroyJoinedPlayerNames();
        for(int i = 0; i < playerNames.Count; i++)
        {
            InstantiateJoinedPlayers(playerNames[i]);
        }

        foreach(string p in playerNames)
        {
            if (!_prevPlayerNames.Contains(p))
            {
                InstantiateAnnoucement(p + "joined the lobby");
            }
        }

        foreach(string p in _prevPlayerNames)
        {
            if (!playerNames.Contains(p))
            {
                InstantiateAnnoucement(p + "left the lobby");
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
        obj.GetComponent<TextMeshProUGUI>().text = name;
    }
    private void InstantiateAnnoucement(string annoucement)
    {

        GameObject obj = Instantiate(announcementTxtPrefab, annoucementContainer);
        obj.GetComponent<TextMeshProUGUI>().text = annoucement;

    }
}
