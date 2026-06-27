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
    public bool isInitialRoundsDone = false;
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
        
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(RegisterAfterSpawn());
        _currentPhase.OnValueChanged += (previousValue, newValue) =>
        {
            onPhaseChanged?.Invoke(newValue);
        };

        onPhaseChanged?.Invoke(_currentPhase.Value);   
    }

    private IEnumerator RegisterAfterSpawn()
    {
        yield return null;
        if (GameData.isOnline && !IsHost)
        {
            Lobby lobby = LobbyManager.instance.CurrentLobby;
            string myName = "";

            foreach (var player in lobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    myName = player.Data["PlayerName"].Value.Trim().Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "");
                    Debug.Log("My name found: " + myName);
                    break;
                }
            }
            if (!IsHost)
            {

                NetworkPlayerManager.instance.RegisterClientServerRpc(myName, NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    public void StartGame()
    {
        if(!IsHost && GameData.isOnline) return;

        Debug.Log("StartGame called, playerNames count: " + GameData.playerNames.Count);

        _currentRound = 1;

        if(GameData.isOnline)
        {
            GameData.playerNames.Clear();
            foreach(var player in LobbyManager.instance.CurrentLobby.Players)
            {
                GameData.playerNames.Add(player.Data["PlayerName"].Value.Trim().Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", ""));
            }
        }
        LobbyManager.instance.StopPolling();
        Debug.Log("After populating, playerNames: " + string.Join(", ", GameData.playerNames));

        PlayerManager.instance.InitilizeGame();
        PlayerManager.instance.ShufflePlayerOrder();


        
        NetworkPlayerManager.instance.PopulatePlayers();
        if (!GameData.isOnline)
        {
            StartWordRevealPhase();
        }
    }

    public void StartWordRevealPhase()
    {
        if(!IsHost && GameData.isOnline) return;
        SetPhase(GamePhase.WordReveal);

        if (GameData.isOnline)
        {
            Timer.instance.StartTimer(10f, StartCluePhase);
            NetworkPlayerManager.instance.StartTimerClientRpc();
        }
    }

    public void StartCluePhase()
    {
        if (!IsHost && GameData.isOnline) return;


        //because we used _currentPlayerIndex for wordReveal in offline mode
        //to check who is accesing the word, so we are restting it here
        _currentPlayerIndex = 0;
        SetPhase(GamePhase.Clue);
    }

    public void StartVoting()
    {
        if (!IsHost && GameData.isOnline) return;

        _currentPlayerIndex = 0;
        SetPhase(GamePhase.Voting);


        VotingManager.instance.Initialize();

        if (GameData.isOnline)
            Timer.instance.StartTimer(GameData.votingDuration, VotingManager.instance.TallyVotes);
    }

    public void EndGame(GameResult result)
    {
        if (!IsHost && GameData.isOnline) return;

        _gameResult = result;
        SetPhase(GamePhase.Result);

    }

    public void NextRound()
    {
        if (!IsHost && GameData.isOnline) return;

        _currentPlayerIndex = 0;

        _currentRound++;
        if (!isInitialRoundsDone)
        {
            if(_currentRound > GameData.roundsCount)
            {
                isInitialRoundsDone = true;
                StartVoting();
            }
            else
            {
                SetPhase(GamePhase.Clue);
            }
        }
        else
        {
            StartVoting();
        }
    }
    public void StartClueAfterVote()
    {
        if (!IsHost && GameData.isOnline) return;

        _currentPlayerIndex = 0;

        PlayerManager.instance.ResetClueStatus();
        SetPhase(GamePhase.Clue);
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
        // In NextPlayerClue
        Debug.Log("NextPlayerClue: index=" + _currentPlayerIndex + " activeCount=" + PlayerManager.instance.GetActivePlayers().Count);


        if (_currentPlayerIndex >= PlayerManager.instance.GetActivePlayers().Count)
        {
            _currentPlayerIndex = 0;
            NextRound();
        }
    }

    public void NextWordRevealPlayer()
    {
        _currentPlayerIndex++;

        if (_currentPlayerIndex >= PlayerManager.instance.GetActivePlayers().Count)
        {
            StartCluePhase();
        }
    }

    public void NextVoter()
    {
        _currentPlayerIndex++;
        onVoterChanged?.Invoke(_currentPlayerIndex);
    }

    public void SetPhase(GamePhase phase)
    {
        _currentPhase.Value = phase;

        if (!GameData.isOnline)
        {
            onPhaseChanged?.Invoke(phase);
        }

    }


    [ClientRpc]
    public void NextPlayerClueClientRpc()
    {
        NextPlayerClue();
    }
}
