using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public class VotingManager : NetworkBehaviour
{
    private Dictionary<Player, int> _votes = new Dictionary<Player, int>();
    private int _votesCount = 0;

    public event Action<Player> onPlayerEliminated;

    public static VotingManager instance;

    public int EligibleVoters => _votes.Count;


    

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
    }


    public void Initialize()
    {

        foreach(Player player in PlayerManager.instance.GetPlayers)
        {
            if(!player.isEliminated)
                _votes[player] = 0;
        }
    }

    public int CastVote(Player player)
    {
        _votes[player] += 1;
        player.hasVoted = true;
        _votesCount++;

        if(_votesCount >= _votes.Count)
        {
            Invoke("TallyVotes", 1f);

        }

        return _votes[player];
    }

    [ServerRpc(RequireOwnership = false)]
    public void CastVoteServerRpc(string playerName)
    {
        Player player = new Player();
        foreach(Player p in PlayerManager.instance.GetPlayers)
        {
            if(p.name == playerName)
            {
                player = p;
                break;
            }
        }

        if (player.hasVoted) return;


        _votes[player] += 1;
        player.hasVoted = true;

        UpdateVoteCountClientRpc(playerName, _votes[player]);
        _votesCount++;

        if (_votesCount >= _votes.Count)
        {
            Invoke("TallyVotes", 1f);

        }

    }

    [ClientRpc]
    public void UpdateVoteCountClientRpc(string playerName, int votesCount)
    {
        VotingPanelUI votingPanelUIScript = FindAnyObjectByType<VotingPanelUI>();
        Transform parent = votingPanelUIScript.GetVotingGridContainer;
        foreach(Transform child in parent)
        {
            VoteBtn voteBtn = child.gameObject.GetComponent<VoteBtn>();
            if(voteBtn.GetPlayer.name == playerName)
            {
                voteBtn.UpdateVoteCount(votesCount);
            }
        }
    }

    public void ResetVotes()
    {
        if (!IsHost && GameData.isOnline) return;
        _votes.Clear();
        _votesCount = 0;
        Initialize();
    }




    public void TallyVotes()
    {
        if (!IsHost && GameData.isOnline) return;

        Player highestVotedPlayer = new Player();

        List<int> playerVotes = _votes.Values.ToList();
        List<Player> highestVotedPlayersList = new List<Player>();


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
            ResetVotes();
            if (!GameData.isOnline)
            {
                onPlayerEliminated?.Invoke(null);
            }
            BroadCastEliminationClientRpc("");
            RoundManager.instance.StartClueAfterVoting();
        }
        else
        {
            //eliminate higest voted player
            highestVotedPlayer.isEliminated = true;
            if(!GameData.isOnline)
            {
                onPlayerEliminated?.Invoke(highestVotedPlayer);
            }
            
            StartCoroutine(FinishVoting(highestVotedPlayer.name));

        }

    }

    public IEnumerator FinishVoting(string name)
    {
        BroadCastEliminationClientRpc(name);

        yield return new WaitForSeconds(3f);

        RoundManager.GameResult result = RoundManager.instance.CheckWinCondition();

        if (result is not RoundManager.GameResult.None)
            RoundManager.instance.EndGame(result);
        else
        {
            RoundManager.instance.StartClueAfterVoting();
        }

        ResetVotes();
    }
    


    public void SkipVote(Player player)
    {
        _votesCount++;
        player.hasVoted = true;
        if(_votesCount >= _votes.Count)
        {
            TallyVotes();
        }
        else
        {
            RoundManager.instance.NextVoter();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void SkipVoteServerRpc(string playerName)
    {

        Player player = new Player();
        foreach (Player p in PlayerManager.instance.GetPlayers)
        {
            if (p.name == playerName)
            {
                player = p;
                break;
            }
        }

        if (player.hasVoted) return;

        _votesCount++;
        player.hasVoted = true;


        if (_votesCount >= _votes.Count)
        {
            TallyVotes();
        }
    }
    [ClientRpc]
    public void BroadCastEliminationClientRpc(string playerName)
    {
        if(playerName == "")
        {
            onPlayerEliminated?.Invoke(null);
        }

        Player player = new Player();
        foreach (Player p in PlayerManager.instance.GetPlayers)
        {
            if (p.name == playerName)
            {
                player = p;
                break;
            }
        }

        if (player.isEliminated)
        {
            onPlayerEliminated?.Invoke(player);
        }
    }
}
