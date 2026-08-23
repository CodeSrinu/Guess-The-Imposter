using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winTextComp;

    public void ShowGameResult(RoundManager.GameResult result)
    {
        StartCoroutine(ShowResult(result));
    }
    private IEnumerator ShowResult(RoundManager.GameResult result)
    {
        
        winTextComp.text = result is RoundManager.GameResult.ImpostersWon ? "Imposters Won" : "Civilians Won";


        yield return new WaitForSeconds(2f);
        if (GameData.isOnline)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
            }
        }
        else
        {
            SceneManager.LoadScene("LobbyCreation");
        }
    }
}
