using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winTextComp;

    public IEnumerator ShowGameResult(RoundManager.GameResult result)
    {
        winTextComp.text = result is RoundManager.GameResult.ImpostersWon ? "Imposters Won" : "Civilians Won";

        yield return new WaitForSeconds(2f);
        UIManager.instance.RestartGame();
    }
}
