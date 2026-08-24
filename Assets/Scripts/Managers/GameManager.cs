using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public enum GameState { Lobby, Playing, Paused, GameOver}

    public static GameManager instance;

    private void Awake()
    {
        Debug.Log($"GameManager.Awake, instance hash before: {(instance != null ? instance.GetHashCode() : -1)}, this hash: {GetHashCode()}");
        if (instance != null && instance != this)
        {
            Debug.LogError($"GameManager destroying duplicate. Surviving instance hash: {instance.GetHashCode()}, destroyed hash: {GetHashCode()}");
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

        if(GameData.isOnline && IsHost)
        {
            RoundManager.instance.StartGame();
        }
    }
}
