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
        Intialize();
        //testing purpose
        AssignRondomValues();
        TallyVotes();
    }


    public void Intialize()
    {
        foreach(Player player in PlayerManager.instance.GetPlayers)
        {
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
    

    //testing purpose
    public void AssignRondomValues()
    {
        foreach (Player player in _votes.Keys.ToList())
        {
            _votes[player] = Random.Range(0, GameData.playersCount);
        }

    }




    public void TallyVotes()
    {
        Player highestVotedPlayer;

        List<int> playerVotes = _votes.Values.ToList();

        foreach (int vote in playerVotes) Debug.Log(vote);

        foreach(Player player in _votes.Keys)
        {
            if(_votes[player] == _votes.Values.Max())
            {
                highestVotedPlayer = player;
            }
        }

    }
}
