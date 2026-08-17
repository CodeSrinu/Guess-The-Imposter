using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public enum GameState { Lobby, Playing, Paused, GameOver}

    public static GameManager instance;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Application.runInBackground = true;
    }

    private void Start()
    {
        if (!GameData.isOnline)
        {
            RoundManager.instance.StartGame();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleGameSceneLoaded;
    }

    private void HandleGameSceneLoaded(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        if (sceneName != "Game") return;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleGameSceneLoaded;

        RoundManager.instance.StartGame();
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleGameSceneLoaded;   
        }
    }
}
