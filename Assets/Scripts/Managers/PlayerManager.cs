using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private List<Player> _players = new List<Player>();

    public List<Player> GetPlayers => _players;
    public bool isEliminated = false;

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


    private void SetUpPlayers()
    {
        foreach(var n in GameData.playerNames)
        {
            Player player = new Player();
            player.name = n;
            _players.Add(player);
        }
    }

    public void AssignImposters()
    {
        for(int i = _players.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            var temp = _players[i];
            _players[i] = _players[randomIndex];
            _players[randomIndex] = temp;
        }

        for(int i = 0; i < GameData.imposterCount; i++)
        {
            _players[i].isImposter = true;
        }

    }


    public void AssignWords()
    {
        WordPair pair = WordManager.instance.GetRandomWordPair();

        foreach(Player player in _players)
        {
            if (player.isImposter) player.assignedWord = pair.imposter;
            else player.assignedWord = pair.normal;
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
}
