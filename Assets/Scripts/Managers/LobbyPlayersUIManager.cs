using TMPro;
using UnityEngine;
using System.Linq;
using System.Collections;

public class LobbyPlayersUIManager : MonoBehaviour
{

    private int poolSize = 10;
    private GameObject[] _pool;
    [SerializeField] private GameObject lobbyPlayerUIPrefab;
    private Transform lobbyPlayersUIContainer;

    private void Start()
    {
        _pool = new GameObject[poolSize];
        lobbyPlayersUIContainer = GetComponent<Transform>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateLobbyPlayerItem();
        }
    }

    private GameObject CreateLobbyPlayerItem()
    {
        GameObject lobbyPlayerItem = Instantiate(lobbyPlayerUIPrefab, lobbyPlayersUIContainer);
        lobbyPlayerItem.SetActive(false);
        _pool.Append(lobbyPlayerItem);
        return lobbyPlayerItem;
    }

    private GameObject GetFreeAnnoucementItem()
    {
        foreach (GameObject item in _pool)
        {
            if (!item.activeSelf) return item;
        }
        return CreateLobbyPlayerItem();
    }

    public void AddPlayerItem(string name, bool isReady = false, bool isThisPlayerHost = false)
    {
        GameObject playerItem = GetFreeAnnoucementItem();
        playerItem.GetComponent<LobbyPlayerItem>().Initialize(isThisPlayerHost, name);

        if (name != "")
            playerItem.GetComponent<LobbyPlayerItem>().SetPlayerReadyStatus(isReady);
        playerItem.SetActive(true);
        StartCoroutine(RemovePlayerItem(playerItem));
    }

    private IEnumerator RemovePlayerItem(GameObject annoucementItem)
    {
        yield return new WaitForSeconds(2f);
        annoucementItem.SetActive(false);
    }
}


