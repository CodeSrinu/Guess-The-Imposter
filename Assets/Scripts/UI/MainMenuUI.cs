using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;

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

        _ = SignIn();
    }

    public void goToLobbyCreationScene()
    {
       SceneManager.LoadScene("LobbyCreation");
    }

    async Task SignIn()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
        catch(Exception e)
        {
            Debug.LogError("Sign in failed: " + e.Message);
        }


        
    }
}
