using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyCreationUI : MonoBehaviour
{
    [SerializeField] private Slider roundsSlider;
    [SerializeField] private Slider playerCountSlider;
    [SerializeField] private Slider imposterCountSlider;
    [SerializeField] private Toggle imposterWordToggle;

    [SerializeField] private Transform playerNamesContainer;
    [SerializeField] private GameObject playerNameInputFiledPrefab;
    [SerializeField] private Button startGameBtn;

    private  int roundsCount = 2;
    private  int playersCount = 3;
    private List<string> playerNames = new List<string>();
    private int imposterCount = 1;
    private bool canImposterHaveWord = false;

    private void Awake()
    {
        playerCountSlider.onValueChanged.AddListener((value) => 
        {
            DestryAllInputFields();

            playersCount = Mathf.RoundToInt(value);
            InstantiateInputFields(playersCount);
        });

        roundsSlider.onValueChanged.AddListener((value) =>
         {
             roundsCount = Mathf.RoundToInt(value);
         });

        imposterCountSlider.onValueChanged.AddListener((value) =>
        {
            imposterCount = Mathf.RoundToInt(value);
        });

        imposterWordToggle.onValueChanged.AddListener((value) =>
        {
            canImposterHaveWord = value;
        });

        startGameBtn.onClick.AddListener(() =>
        {
            SaveDataToGameDataClass();
            SceneManager.LoadScene("Game");
        });
    }

    public void InstantiateInputFields(int count)
    {
        for(int i = 0; i < count; i++)
        {
            var prefab = Instantiate(playerNameInputFiledPrefab);
            prefab.transform.SetParent(playerNamesContainer, false);
        }
    }

    public void DestryAllInputFields()
    {
        if (playerNamesContainer.childCount <= 0) return;

        foreach (Transform child in playerNamesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void SaveDataToGameDataClass()
    {
        foreach(Transform child  in playerNamesContainer)
        {
            playerNames.Add(child.GetComponentInChildren<TMP_InputField>().text);
        }
        GameData.playerNames = playerNames;
        GameData.playersCount = playersCount;
        GameData.roundsCount = roundsCount;
        GameData.imposterCount = imposterCount;
        GameData.canImposterHaveWord = canImposterHaveWord;
    }
}
