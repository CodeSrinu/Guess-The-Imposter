using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VoteBtn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _voterBtnTextComp;
    [SerializeField] private TextMeshProUGUI _castedVotesTextComp;
    private Player _player;


    private void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            //if (!_player.hasVoted) 
            { 
                int count = VotingManager.instance.CastVote(_player);
                _castedVotesTextComp.text = count.ToString();
                RoundManager.instance.NextVoter();

            }
        });
    }


    public void SetUp(Player player)
    {
        _player = player;
        _voterBtnTextComp.text = _player.name;
    }

}
