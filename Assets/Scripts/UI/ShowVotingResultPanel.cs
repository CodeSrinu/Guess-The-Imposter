using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowVotingResultPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _eliminatedPlayerTxtComp;
    [SerializeField] private TextMeshProUGUI _votedPlayerImposterStatusTxtComp;
    [SerializeField] private TextMeshProUGUI _imposterRemainingTxtComp;



    public void SetVotingPanelResult(Player player)
    {
        _eliminatedPlayerTxtComp.gameObject.SetActive(true);
        _imposterRemainingTxtComp.gameObject.SetActive(true);

        _eliminatedPlayerTxtComp.text = player.name + " is Eliminated";
        _votedPlayerImposterStatusTxtComp.text = player.isImposter ? player.name + " is Imposter" : player.name + " is not Imposter";
        _imposterRemainingTxtComp.text = PlayerManager.instance.GetAllImposter().Count.ToString() + " Imposter remaining";
    }

    public void SetTieResult()
    {
        _eliminatedPlayerTxtComp.gameObject.SetActive(false);
        _imposterRemainingTxtComp.gameObject.SetActive(false);

        _votedPlayerImposterStatusTxtComp.text = "Nobody eliminated Game will Continue...";
    }
}
