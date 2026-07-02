using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;


public class RoundManager : NetworkBehaviour
{
    private NetworkVariable<int> _currentPlayerIndex = new NetworkVariable<int>();
    private NetworkVariable<int> _currentRound = new NetworkVariable<int>();
    public enum GamePhase { WordReveal, Clue, Voting, Result}
    public enum GameResult { None, ImpostersWon, CiviliansWon}

    private NetworkVariable<GamePhase> _currentPhase = new NetworkVariable<GamePhase>();
    private GameResult _gameResult;

    public event Action<GamePhase> onPhaseChanged;
    public event Action<int> onVoterChanged;
    public bool isInitialRoundsDone = false;
    public GamePhase currentPhase => _currentPhase.Value;

    public int CurrentPlayerIndex => _currentPlayerIndex.Value;
    public int CurrentRound => _currentRound.Value;

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
        if (IsHost) return;

        LoadingScreenUI.instance.StartLoading();
    }

    public override void OnNetworkSpawn()
    {
        if(GameData.isOnline && !IsHost)
        {
            StartCoroutine(RegisterAfterSpawn());
        }


        _currentPhase.OnValueChanged += (previousValue, newValue) =>
        {
            onPhaseChanged?.Invoke(newValue);
        };

        _currentPlayerIndex.OnValueChanged += (previousValue, newValue) =>
        {

            if (_currentPhase.Value == GamePhase.Clue)
            {
                UIManager.instance.SetUpCluePanel();
            }
            else if(_currentPhase.Value == GamePhase.Voting)
            {
                UIManager.instance.HandleVoterChanged(newValue);
            }
        };

    }

    private IEnumerator RegisterAfterSpawn()
    {
        Debug.Log("RegisterAfterSpawn: waiting for NetworkPlayerManager");
        yield return new WaitUntil(() => NetworkPlayerManager.instance != null && NetworkPlayerManager.instance.IsSpawned && NetworkPlayerManager.instance.gameObject.scene.isLoaded && LobbyManager.instance.CurrentLobby != null);

        yield return null;


        Debug.Log("RegisterAfterSpawn: NetworkPlayerManager ready, registering as " + GameData.devicePlayerName);

        if (GameData.isOnline && !IsHost)
        {
            Lobby lobby = LobbyManager.instance.CurrentLobby;
            string myName = "";

            foreach (var player in lobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    myName = player.Data["PlayerName"].Value.Trim().Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "");
                    GameData.devicePlayerName = myName;
                    break;
                }
            }
            if (!IsHost)
            {

                NetworkPlayerManager.instance.RegisterClientServerRpc(myName, NetworkManager.Singleton.LocalClientId);
                LoadingScreenUI.instance.StopLoading();
            }
        }
    }

    public void StartGame()
    {
        if(!IsHost && GameData.isOnline) return;

        NetworkPlayerManager.instance.ResetRegistrationState();

        Debug.Log("StartGame called, playerNames count: " + GameData.playerNames.Count);

        _currentRound.Value = 1;

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
            NetworkPlayerManager.instance.StartTimerClientRpc(10f);
        }
    }

    public void StartCluePhase()
    {
        if (!IsHost && GameData.isOnline) return;


        //because we used _currentPlayerIndex for wordReveal in offline mode
        //to check who is accesing the word, so we are restting it here
        _currentPlayerIndex.Value = 0;
        SetPhase(GamePhase.Clue);
    }

    public void StartVoting()
    {
        if (!IsHost && GameData.isOnline) return;

        _currentPlayerIndex.Value = 0;
        SetPhase(GamePhase.Voting);


        VotingManager.instance.Initialize();

        if (GameData.isOnline)
        {
            Timer.instance.StartTimer(GameData.votingDuration, VotingManager.instance.TallyVotes);
            NetworkPlayerManager.instance.StartTimerClientRpc(GameData.votingDuration);
        }
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

        _currentPlayerIndex.Value = 0;

        _currentRound.Value++;
        if (!isInitialRoundsDone)
        {
            if(_currentRound.Value > GameData.roundsCount)
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

        _currentPlayerIndex.Value = 0;

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
        Debug.Log("NextPlayerClue is called by host");
        _currentPlayerIndex.Value++;
        


        if (_currentPlayerIndex.Value >= PlayerManager.instance.GetActivePlayers().Count)
        {
            _currentPlayerIndex.Value = 0;
            NextRound();
        }
    }

    public void NextWordRevealPlayer()
    {
        _currentPlayerIndex.Value++;

        if (_currentPlayerIndex.Value >= PlayerManager.instance.GetActivePlayers().Count)
        {
            StartCluePhase();
        }
    }

    public void NextVoter()
    {
        _currentPlayerIndex.Value++;
        onVoterChanged?.Invoke(_currentPlayerIndex.Value);
    }

    public void SetPhase(GamePhase phase)
    {
        _currentPhase.Value = phase;
        onPhaseChanged?.Invoke(phase);
    }


    [ServerRpc(RequireOwnership = false)]
    public void NextPlayerClueServerRpc()
    {
        NextPlayerClue();
    }
}
