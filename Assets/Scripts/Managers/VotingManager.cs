using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VotingManager : MonoBehaviour
{
    private Dictionary<Player, int> _votes = new Dictionary<Player, int>();
    private int _votesCount = 0;

    public static VotingManager instance;

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
        Initialize();
    }


    public void Initialize()
    {
        foreach(Player player in PlayerManager.instance.GetPlayers)
        {
            if(!player.isEliminated)
                _votes[player] = 0;
        }
    }

    public void CastVote(Player player)
    {
        _votes[player] += 1;
        player.hasVoted = true;
        _votesCount++;

        
        if(_votesCount >= GameData.playersCount)
        {
            TallyVotes();
        }
    }
    

    public void ResetVotes()
    {
        _votes.Clear();
        _votesCount = 0;
        Initialize();
    }




    public void TallyVotes()
    {
        Player highestVotedPlayer = new Player();

        List<int> playerVotes = _votes.Values.ToList();
        List<Player> highestVotedPlayersList = new List<Player>();

        foreach (int vote in playerVotes) Debug.Log(vote);

        foreach(Player player in _votes.Keys)
        {
            if(_votes[player] == _votes.Values.Max())
            {
                highestVotedPlayer = player;
                highestVotedPlayersList.Add(player);
            }
        }

        if(highestVotedPlayersList.Count > 1)
        {
            //tie
            RoundManager.instance.NextRound();
            ResetVotes();
        }
        else
        {
            //eliminate higest voted player
            highestVotedPlayer.isEliminated = true;
            RoundManager.GameResult result = RoundManager.instance.CheckWinCondition();
            
            if(result is not RoundManager.GameResult.None)
                RoundManager.instance.EndGame(result);
            else
                RoundManager.instance.NextRound();
            
            ResetVotes();

        }

    }
}
