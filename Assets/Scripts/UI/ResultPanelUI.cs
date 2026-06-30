using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winTextComp;

    public IEnumerator ShowGameResult(RoundManager.GameResult result)
    {
        winTextComp.text = result is RoundManager.GameResult.ImpostersWon ? "Imposters Won" : "Civilians Won";

        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("LobbyCreation");
    }
}
