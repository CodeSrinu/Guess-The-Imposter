using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    private int _currentPlayerIndex;
    private int _currentRound;
    public enum GamePhase { WordReveal, Clue, Voting, Result}
    public enum GameResult { None, ImpostersWon, CiviliansWon}

    private GamePhase _currentPhase;
    private GameResult _gameResult;

    public event Action<GamePhase> onPhaseChanged;

    public GamePhase currentPhase => _currentPhase;

    public int CurrentPlayerIndex => _currentPlayerIndex;

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

    public void StartGame()
    {
        _currentRound = 1;
        PlayerManager.instance.InitilizeGame();
        _currentPhase = GamePhase.WordReveal;
        onPhaseChanged?.Invoke(_currentPhase);

        if(GameData.isOnline)
            Timer.instance.StartTimer(10f, StartCluePhase);
    }


    public void StartCluePhase()
    {
        //because we used _currentPlayerIndex for wordReveal in offline mode
        //to check who is accesing the word, so we are restting it here
        _currentPlayerIndex = 0; 


        _currentPhase = GamePhase.Clue;
        onPhaseChanged?.Invoke(_currentPhase);
    }

    public void StartVoting()
    {
        _currentPhase = GamePhase.Voting;
        onPhaseChanged?.Invoke(_currentPhase);

        if(GameData.isOnline)
            Timer.instance.StartTimer(GameData.votingDuration, VotingManager.instance.TallyVotes);
    }

    public void EndGame(GameResult result)
    {
        _gameResult = result;
        _currentPhase = GamePhase.Result;
        onPhaseChanged?.Invoke(_currentPhase);
    }

    public void NextRound()
    {
        _currentRound++;

        if(_currentRound > GameData.roundsCount)
        {
            StartVoting();
        }
        else
        {
            PlayerManager.instance.ResetClueStatus();
            _currentPhase = GamePhase.Clue;
            onPhaseChanged?.Invoke(_currentPhase);
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
}
