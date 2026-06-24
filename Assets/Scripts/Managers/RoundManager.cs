using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;


public class RoundManager : NetworkBehaviour
{
    private int _currentPlayerIndex;
    private int _currentRound;
    public enum GamePhase { WordReveal, Clue, Voting, Result}
    public enum GameResult { None, ImpostersWon, CiviliansWon}

    private NetworkVariable<GamePhase> _currentPhase = new NetworkVariable<GamePhase>();
    private GameResult _gameResult;

    public event Action<GamePhase> onPhaseChanged;
    public event Action<int> onVoterChanged;

    public GamePhase currentPhase => _currentPhase.Value;

    public int CurrentPlayerIndex => _currentPlayerIndex;
    public int CurrentRound => _currentRound;

    public GameResult result => _gameResult;

    public static RoundManager instance;

    


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
        if (GameData.isOnline)
        {
            Lobby lobby = LobbyManager.instance.CurrentLobby;
            string myName = "";

            if (IsHost)
            {
                myName = GameData.playerNames[0];
            }
            else
            {
                foreach (var player in lobby.Players)
                {
                    if (player.Id == AuthenticationService.Instance.PlayerId)
                    {
                        myName = player.Data["PlayerName"].Value;
                        Debug.Log("My name found: " + myName);
                        break;
                    }
                }
            }

            NetworkPlayerManager.instance.RegisterClientServerRpc(myName, NetworkManager.Singleton.LocalClientId);
        }
    }

    public override void OnNetworkSpawn()
    {
        _currentPhase.OnValueChanged += (previousValue, newValue) =>
        {
            onPhaseChanged?.Invoke(newValue);
        };

        onPhaseChanged?.Invoke(_currentPhase.Value);


        
    }


    public void StartGame()
    {

        Debug.Log("Player names count: " + GameData.playerNames.Count);
        foreach (var n in GameData.playerNames) Debug.Log("Player name: " + n);

        _currentRound = 1;
        PlayerManager.instance.InitilizeGame();
        PlayerManager.instance.ShufflePlayerOrder();

        if(!IsHost) return;

        LobbyManager.instance.StopPolling();
        NetworkPlayerManager.instance.PopulatePlayers();
        

        if(GameData.isOnline)
            Timer.instance.StartTimer(10f, StartCluePhase);
    }

    public void StartWorRevealPhase()
    {
        if(!IsHost) return;
        _currentPhase.Value = GamePhase.WordReveal;
    }

    public void StartCluePhase()
    {
        if (!IsHost) return;


        //because we used _currentPlayerIndex for wordReveal in offline mode
        //to check who is accesing the word, so we are restting it here
        _currentPlayerIndex = 0; 
        _currentPhase.Value = GamePhase.Clue;
    }

    public void StartVoting()
    {
        if (!IsHost) return;

        _currentPlayerIndex = 0;
        _currentPhase.Value = GamePhase.Voting;


        VotingManager.instance.Initialize();

        if (GameData.isOnline)
            Timer.instance.StartTimer(GameData.votingDuration, VotingManager.instance.TallyVotes);
    }

    public void EndGame(GameResult result)
    {
        if (!IsHost) return;

        _gameResult = result;
        _currentPhase.Value = GamePhase.Result;

    }

    public void NextRound()
    {
        if (!IsHost) return;

        _currentRound++;

        if(_currentRound > GameData.roundsCount)
        {
            StartVoting();
        }
        else
        {
            PlayerManager.instance.ResetClueStatus();
            _currentPhase.Value = GamePhase.Clue;
        }
    }

    public GameResult CheckWinCondition()
    {
        int remainingImposters = 0;
        int remainingCivilians = 0;
        foreach (Player player in PlayerManager.instance.GetPlayers)
        {
            if (!player.isEliminated)
            {
                if (player.isImposter)
                {
                    remainingImposters++;
                }
                else
                {
                    remainingCivilians++;
                }
            }
        }


        if (remainingImposters >= remainingCivilians)
        {
            Debug.Log("Imposters Won");
            return GameResult.ImpostersWon;
        }
        else if (remainingImposters <= 0)
        {
            Debug.Log("Civilians Won");
            return GameResult.CiviliansWon;
        }
        else
        {
            return GameResult.None;
        }
        
    }

    public void NextPlayerClue()
    {
        _currentPlayerIndex++;

        if(_currentPlayerIndex >= GameData.playersCount)
        {
            _currentPlayerIndex = 0;
            NextRound();
        }
    }

    public void NextWordRevealPlayer()
    {
        _currentPlayerIndex++;
        if(_currentPlayerIndex >= GameData.playersCount)
        {
            StartCluePhase();
        }
    }

    public void NextVoter()
    {
        _currentPlayerIndex++;
        onVoterChanged?.Invoke(_currentPlayerIndex);
    }
}
