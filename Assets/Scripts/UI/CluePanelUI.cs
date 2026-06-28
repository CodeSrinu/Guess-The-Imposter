using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
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



    public void SetUp(string currentPlayerName, int currentRound)
    {
        _currentPlayerGivingClueTxtComp.text = currentPlayerName + "\'s Turn";
        _currentRoundTxtComp.text = "Round " + currentRound;


        string myName = GameData.devicePlayerName;
        if (GameData.isOnline)
        {
            _nextPlayerBtn.gameObject.SetActive(myName == currentPlayerName);
        }
        else
        {
            _nextPlayerBtn.gameObject.SetActive(true);
        }
    }




    public void OpenVotingTable()
    {

        RoundManager.instance.StartVoting();


    }

}
