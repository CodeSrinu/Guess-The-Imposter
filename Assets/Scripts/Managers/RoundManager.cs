using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    private int _currentPlayerIndex;
    private int _currentRound;
    public enum GamePhase { WordReveal, Clue, Voting, Result}

    private GamePhase _currentPhase;

    public GamePhase currentPhase => _currentPhase;

    public static RoundManager instance;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(instance );
            return;
        }

        instance = this;
    }

    private void Start()
    {
        _currentRound = 1;
        PlayerManager.instance.InitilizeGame();
        _currentPhase = GamePhase.WordReveal;
    }


    public void StartVoting()
    {
        _currentPhase = GamePhase.Voting;

        if(GameData.isOnline)
            Timer.instance.StartTimer(GameData.votingDuration, VotingManager.instance.TallyVotes);
    }

    public void EndGame()
    {
        _currentPhase = GamePhase.Result;
    }

    public void NextRound()
    {
        _currentRound++;

        if(_currentRound > GameData.roundsCount)
        {
            CheckWinCondition();
        }
        else
        {
            PlayerManager.instance.ResetClueStatus();
            _currentPhase = GamePhase.Clue;
        }
    }

    public void CheckWinCondition()
    {
        int imposterCount = 0;
        foreach(Player player in PlayerManager.instance.GetPlayers)
        {
            if(player.isImposter)
                imposterCount++;
        }

        if(imposterCount <= 0)
        {
            Debug.Log("Civilians Won");
        }
        else
        {
            Debug.Log("Imposters Won");
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
}
