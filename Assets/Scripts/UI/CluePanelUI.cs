using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CluePanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentRoundTxtComp;
    [SerializeField] private TextMeshProUGUI _currentPlayerGivingClueTxtComp;
    [SerializeField] private Button _nextPlayerBtn;
    [SerializeField] private Button _votingTableBtn;

    public Button GetNextPlayerBtn => _nextPlayerBtn;
    public Button GetVotingTableBtn => _votingTableBtn;



    public void SetPlayerName(string playerName)
    {
        _currentPlayerGivingClueTxtComp.text = playerName + "\'s Turn";
    }

    public void SetCurrentRound(int currentRound)
    {
        _currentRoundTxtComp.text = "Round" + currentRound;
    }

    public void OpenVotingTable()
    {

        RoundManager.instance.StartVoting();


    }

}
