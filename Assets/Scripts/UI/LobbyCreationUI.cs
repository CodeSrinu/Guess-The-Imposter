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
    [SerializeField] private TMP_InputField votingDurationInputFeild;
    [SerializeField] private Toggle imposterWordToggle;
    [SerializeField] private Toggle isOnlineToggle;

    [SerializeField] private Transform playerNamesContainer;
    [SerializeField] private GameObject playerNameInputFiledPrefab;
    [SerializeField] private Button startGameBtn;

    private  int roundsCount = 2;
    private  int playersCount = 3;
    private List<string> playerNames = new List<string>();
    private int imposterCount = 1;
    private int votingDuration = 30;
    private bool isOnline = false;
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

        votingDurationInputFeild.onValueChanged.AddListener((value) =>
        {
            int parsedValue = int.Parse(value);
            votingDuration = Mathf.RoundToInt(parsedValue);
            if (votingDuration > 60)
                votingDuration = 60;
        });

        isOnlineToggle.onValueChanged.AddListener((value) =>
        {
            isOnline = value;
        });

        startGameBtn.onClick.AddListener(() =>
        {
            SaveDataToGameDataClass();
            SceneManager.LoadScene("Game");
        });

        votingDurationInputFeild.contentType = TMP_InputField.ContentType.IntegerNumber;
        votingDurationInputFeild.ForceLabelUpdate();
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
        GameData.votingDuration = votingDuration;
        GameData.isOnline = isOnline;
    }
}
