using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VotingPanelUI : MonoBehaviour
{
    [SerializeField] private Transform _votingGridContainer;
    [SerializeField] private TextMeshProUGUI voterNameTextComp;
    [SerializeField] private GameObject _votingBtnPrefab;
    [SerializeField] private Button _skipVoteBtn;

    public Button GetSkipBtn => _skipVoteBtn;


    public void DestroyAllVotingBtns()
    {
        foreach(Transform child in _votingGridContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void InstantiateVotingBtns(int btnsCount, List<Player> players)
    {
        for(int i = 0; i < btnsCount; i++)
        {
            GameObject btn = Instantiate(_votingBtnPrefab, _votingGridContainer);
            btn.GetComponent<VoteBtn>().SetUp(players[i]);
            btn.GetComponent<VoteBtn>().SetVoterName();
        }
    }

    public void SetWhosTurn(string name)
    {
        voterNameTextComp.text = name + "\' turn to vote";
    }
}
