using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button newGameBtn;
    [SerializeField] private Button exitGameBtn;

    private void Awake()
    {
        newGameBtn.onClick.AddListener(() => { 
            goToLobbyCreationScene();
        });
        exitGameBtn.onClick.AddListener(() => {
            Application.Quit();
        });
    }

    public void goToLobbyCreationScene()
    {
       SceneManager.LoadScene("LobbyCreation");
    }
}
