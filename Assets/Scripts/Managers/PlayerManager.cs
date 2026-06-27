using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private List<Player> _players = new List<Player>();

    public List<Player> GetPlayers => _players;

    public static PlayerManager instance;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }


    public List<Player> GetActivePlayers()
    {
        var list = new List<Player>();
        foreach (Player player in PlayerManager.instance.GetPlayers)
        {
            if (!player.isEliminated)
                list.Add(player);
        }

        return list;
    }

    private void SetUpPlayers()
    {
        foreach(var n in GameData.playerNames)
        {
            Player player = new Player();
            player.name = n;
            _players.Add(player);
        }
    }

    public void SetUpPlayersOnly()
    {
        _players.Clear();
        SetUpPlayers();

    }

    public void AssignImposters()
    {
        ShufflePlayerOrder();

        for(int i = 0; i < GameData.imposterCount; i++)
        {
            _players[i].isImposter = true;
        }

    }


    public void AssignWords()
    {
        WordPair pair = WordManager.instance.GetRandomWordPair();

        Debug.Log("AssignWords pair: normal=" + pair.normal + " imposter=" + pair.imposter);
        foreach (Player player in _players)
        {
            if (player.isImposter) player.assignedWord = pair.imposter;
            else player.assignedWord = pair.normal;
            Debug.Log(player.name + " assigned word: " + player.assignedWord);
        }
    }

    public void ResetClueStatus()
    {
        foreach(Player player in _players) player.hasGiveClue = false;
    }


    public void InitilizeGame()
    {
        _players.Clear();
        SetUpPlayers();
        AssignImposters();
        AssignWords();
    }

    public List<Player> GetAllImposter()
    {
        List<Player> list = new List<Player>();

        foreach (Player player in _players)
        {
            if (!player.isEliminated)
            {
                if(player.isImposter)
                    list.Add(player);
            }
        }

        return list;
    }

    public void ShufflePlayerOrder()
    {
        for (int i = _players.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            var temp = _players[i];
            _players[i] = _players[randomIndex];
            _players[randomIndex] = temp;
        }
    }
}
