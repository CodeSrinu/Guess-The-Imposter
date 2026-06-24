using Unity.Netcode;
using UnityEngine;


public class GameManager : NetworkBehaviour
{
    public enum GameState { Lobby, Playing, Paused, GameOver}
    private GameState _state;

    public static GameManager instance;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
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
        RoundManager.instance.StartGame();
    }

    
}
