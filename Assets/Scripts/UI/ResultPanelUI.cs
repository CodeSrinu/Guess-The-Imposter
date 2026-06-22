using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winTextComp;

    public void ShowGameResult(RoundManager.GameResult result)
    {
        winTextComp.text = result is RoundManager.GameResult.ImpostersWon ? "Imposters Won" : "Civilians Won";
    }
}
