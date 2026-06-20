using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
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
        RoundManager.instance.StartGame();
    }
}
