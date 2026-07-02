using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameErrMsgTxt;
    [SerializeField] private TextMeshProUGUI roomCodeErrMsgTxt;

    [SerializeField] private Button createGameBtn;
    [SerializeField] private Button joinGameBtn;
    [SerializeField] private Button exitGameBtn;
    [SerializeField] private Button createGameCancelBtn;
    [SerializeField] private Button joinGameCancelBtn;
    [SerializeField] private Button onlineBtn;
    [SerializeField] private Button localBtn;
    [SerializeField] private Button joinRoomBtn;

    [SerializeField] private TMP_InputField playerNameInputFeildComp;
    [SerializeField] private TMP_InputField roomCodeInputFeildComp;

    [SerializeField] private GameObject onlineStatusPanel;
    [SerializeField] private GameObject joinRoomPanel;

    private void Awake()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        if(LobbyManager.instance != null)
        {
            LobbyManager.instance.StopPolling();
        }

        createGameBtn.onClick.AddListener(() =>
        {
            onlineStatusPanel.SetActive(true);
        });

        createGameCancelBtn.onClick.AddListener(() => {
            onlineStatusPanel.SetActive(false);
        });

        joinGameBtn.onClick.AddListener(() =>
        {
            joinRoomPanel.SetActive(true);
        });

        joinGameCancelBtn.onClick.AddListener(() =>
        {
            joinRoomPanel.SetActive(false); 
        });
        
        exitGameBtn.onClick.AddListener(() => {
            Application.Quit();
        });


        onlineBtn.onClick.AddListener(() =>
        {
            LobbyManager.instance.IsOnline = true;
            goToLobbyCreationScene();
        });

        localBtn.onClick.AddListener(() =>
        {
            LobbyManager.instance.IsOnline = false;
            goToLobbyCreationScene();
        });

        playerNameErrMsgTxt.gameObject.SetActive(false);
        roomCodeErrMsgTxt.gameObject.SetActive(false);

        joinRoomBtn.onClick.AddListener(() => {
            if (IsValidName())
            {
                playerNameErrMsgTxt.gameObject.SetActive(false);
                _ = JoinLobbyFlow();
            }
            else
            {
                playerNameErrMsgTxt.gameObject.SetActive(true);
            }
        });


        _ = SignIn();
    }

    public void goToLobbyCreationScene()
    {
       SceneManager.LoadScene("LobbyCreation");
    }

    public void goToLobbyScene()
    {
        SceneManager.LoadScene("Lobby");
    }


    public bool IsValidName()
    {
        string name = playerNameInputFeildComp.text.Trim();
        return name.Length >= 2;

    }

    async Task SignIn()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
        catch(Exception e)
        {
            Debug.LogError("Sign in failed: " + e.Message);
        }
    }

    public async Task JoinLobbyFlow()
    {
        string roomCode = roomCodeInputFeildComp.text;
        string playerName = playerNameInputFeildComp.text;
        bool result = await LobbyManager.instance.JoinLobby(roomCode, playerName);
        
        if(result)
        {
            LoadingScreenUI.instance.StartLoading();
            roomCodeErrMsgTxt.gameObject.SetActive(false); 
            GameData.isOnline = true;
            goToLobbyScene();
        }
        else
        {
            roomCodeErrMsgTxt.gameObject.SetActive(true);
        }
    }

}
