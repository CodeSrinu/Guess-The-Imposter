using System;
using System.Collections;
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
    private NetworkVariable<GameResult> _gameResult = new NetworkVariable<GameResult>(GameResult.None);

    public event Action<GamePhase> onPhaseChanged;
    public event Action<int> onVoterChanged;
    public bool isInitialRoundsDone = false;
    public GamePhase currentPhase => _currentPhase.Value;
    private NetworkVariable<int> _remianingImposters = new NetworkVariable<int>();
    public int remainingImposters => _remianingImposters.Value;
    public int CurrentPlayerIndex => _currentPlayerIndex.Value;
    public int CurrentRound => _currentRound.Value;

    public GameResult result => _gameResult.Value;

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
        VotingManager.instance.onPlayerEliminated += HandlePlayerEliminated;
        Debug.Log("[CLIENT] RoundManager subscribed to elimination event!");
        if (GameData.isOnline) LoadingScreenUI.instance.StartLoading();

        //if (!IsHost) return;
    }

    private void HandlePlayerEliminated(Player eliminatedPlayer)
    {
        if(eliminatedPlayer == null) return;

        if (eliminatedPlayer.isImposter)
        {
            Debug.Log($"{_remianingImposters.Value} imposters count");
            //_remianingImposters.Value--;
        }
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
        float elapsed = 0f;
        float timeout = 10f;

        while (elapsed < timeout)
        {
            bool npmReady = NetworkPlayerManager.instance != null;
            bool spawned = npmReady && NetworkPlayerManager.instance.IsSpawned;
            bool sceneReady = npmReady && NetworkPlayerManager.instance.gameObject.scene.isLoaded;
            bool lobbyReady = LobbyManager.instance != null && LobbyManager.instance.CurrentLobby != null;

            if (npmReady && spawned && sceneReady && lobbyReady)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if(elapsed >= timeout)
        {
            Debug.LogError($"RegisterAfterSpawn TIMEOUT — npm:{NetworkPlayerManager.instance != null} " +
                            $"spawned:{NetworkPlayerManager.instance?.IsSpawned} " +
                            $"sceneLoaded:{NetworkPlayerManager.instance?.gameObject.scene.isLoaded} " +
                            $"lobby:{LobbyManager.instance?.CurrentLobby != null}");

            LoadingScreenUI.instance.ShowLoadingError("Failed To Join Game, Please Try Again!");
            yield break;
        }

        Debug.Log("RegisterAfterSpawn: NetworkPlayerManager ready, registering as " + GameData.devicePlayerName);

        if (GameData.isOnline && !IsHost)
        {
            Lobby lobby = LobbyManager.instance.CurrentLobby;
            string myName = "";


            if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId))
            {
                Debug.LogError("RegisterAfterSpawn: PlayerId not ready, cannot match lobby player");
                LoadingScreenUI.instance.ShowLoadingError("Sign In not complete, please restart");
                yield break;
            }


            foreach (var player in lobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    myName = player.Data["PlayerName"].Value.Trim().Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "");
                    GameData.devicePlayerName = myName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(myName))
            {
                Debug.LogError("RegisterAfterSpawn: could not find matching player in lobby data");
                LoadingScreenUI.instance.ShowLoadingError("failed to sync player data, please try again");
                yield break;
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
        if (GameData.isOnline && !IsHost) return;

        NetworkPlayerManager.instance.ResetRegistrationState();

        _currentRound.Value = 1;

        LobbyManager.instance.StopPolling();
        PlayerManager.instance.InitilizeGame();
        PlayerManager.instance.ShufflePlayerOrder();

        _remianingImposters.Value = PlayerManager.instance.GetAllImposter().Count;
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

    [ServerRpc(RequireOwnership = false)]
    public void StartVotingServerRpc()
    {
        StartVoting();
    }

    public void EndGame(GameResult result)
    {
        if (!IsHost && GameData.isOnline) return;

        _gameResult.Value = result;
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
    public GameResult CheckWhoWon()
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
            return GameResult.ImpostersWon;
        }
        else if (remainingImposters <= 0)
        {
            return GameResult.CiviliansWon;
        }
        else
        {
            return GameResult.None;
        }
        
    }

    public void NextPlayerClue()
    {
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
