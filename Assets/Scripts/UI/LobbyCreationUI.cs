using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


    [SerializeField] private Transform playerNamesContainer;
    [SerializeField] private GameObject playerNameInputFiledPrefab;
    [SerializeField] private Button startGameBtn;

    [SerializeField] private TextMeshProUGUI roundsSliderTxt;
    [SerializeField] private TextMeshProUGUI playerCountSliderTxt;
    [SerializeField] private TextMeshProUGUI imposterCountSliderTxt;


    private  int roundsCount = 2;
    private  int playersCount = 3;
    private List<string> playerNames = new List<string>();
    private int imposterCount = 1;
    private int votingDuration = 30;
    private bool canImposterHaveWord = false;

    private void Awake()
    {
        if (LobbyManager.instance.IsOnline)
        {
            InstantiateInputFields(1);
        }
        else
        {
            string namesString = PlayerPrefs.GetString("PlayerNamesTemplate", "");
            List<string> prevPlayerNames = namesString.Split(",").ToList();
            if(prevPlayerNames.Count > 0)
            {
                InstantiateInputFields(prevPlayerNames.Count);
                playerCountSlider.value = prevPlayerNames.Count;
                playerCountSliderTxt.text = prevPlayerNames.Count.ToString();
                playersCount = prevPlayerNames.Count;
            }
            else
            {
                InstantiateInputFields(playersCount);
            }
            SetThePlayerNamesTemplate();
        }

        playerCountSlider.onValueChanged.AddListener((value) =>
        {

            int intValue = Mathf.RoundToInt(value);
            playerCountSliderTxt.text = intValue.ToString();
            playersCount = intValue;

            if (!LobbyManager.instance.IsOnline)
            {
                AdjustInputFeildCount(intValue);
                SetThePlayerNamesTemplate();
            }


            imposterCountSlider.maxValue = Mathf.RoundToInt(intValue / 2);
        });

        roundsSlider.onValueChanged.AddListener((value) =>
         {
             int intValue = Mathf.RoundToInt(value);
             roundsSliderTxt.text = intValue.ToString();
             roundsCount = intValue;
         });

        imposterCountSlider.onValueChanged.AddListener((value) =>
        {
            int intValue = Mathf.RoundToInt(value);
            imposterCountSliderTxt.text = intValue.ToString();
            imposterCount = intValue;
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


        startGameBtn.onClick.AddListener(() =>
        {
            if (playerNamesContainer.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text.Trim() != "")

            if (LobbyManager.instance.IsOnline)
            {
                _ = StartOnlineGameFlow();
            }
            else
            {
                SaveDataToGameDataClass();
                SavePlayerNamesTemplate();
                SceneManager.LoadScene("Game");

            }
        });

        votingDurationInputFeild.contentType = TMP_InputField.ContentType.IntegerNumber;
        votingDurationInputFeild.text = votingDuration.ToString();
        votingDurationInputFeild.ForceLabelUpdate();
        imposterCountSliderTxt.text = imposterCount.ToString();
        playerCountSliderTxt.text = playersCount.ToString();
        roundsSliderTxt.text = roundsCount.ToString();


    }

    public void AdjustInputFeildCount(int targetCount)
    {
        int currentCount = playerNamesContainer.childCount;

        if(targetCount > currentCount)
        {
            InstantiateInputFields(targetCount - currentCount);
        }
        else if(targetCount < currentCount) 
        {
            for(int i = currentCount - 1;i >= targetCount; i--)
            {
                Destroy(playerNamesContainer.GetChild(i).gameObject);
            }
        }
    }

    public void InstantiateInputFields(int count)
    {
        for(int i = 0; i < count; i++)
        {
            var prefab = Instantiate(playerNameInputFiledPrefab);
            prefab.transform.SetParent(playerNamesContainer, false);
        }
    }


    public void SaveDataToGameDataClass()
    {
        foreach(Transform child  in playerNamesContainer)
        {
            playerNames.Add(child.GetComponentInChildren<TMP_InputField>().text.Trim().Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", ""));
        }
        GameData.playerNames = playerNames;
        GameData.playersCount = playersCount;
        GameData.roundsCount = roundsCount;
        GameData.imposterCount = imposterCount;
        GameData.canImposterHaveWord = canImposterHaveWord;
        GameData.votingDuration = votingDuration;
        GameData.isOnline = LobbyManager.instance.IsOnline;
    }


    private async Task StartOnlineGameFlow()
    {
        SaveDataToGameDataClass();
        string hostName = playerNamesContainer.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text;
        await LobbyManager.instance.StartOnlineGame(hostName);
        SceneManager.LoadScene("Lobby");
    }


    public void SavePlayerNamesTemplate()
    {
        string playerNamesString = string.Join(",", GameData.playerNames);
        PlayerPrefs.SetString("PlayerNamesTemplate", playerNamesString);
        PlayerPrefs.Save();
    }

    public void SetThePlayerNamesTemplate()
    {
        string namesString = PlayerPrefs.GetString("PlayerNamesTemplate", "");
        List<string> playerNames = namesString.Split(",").ToList();
        int i = 0;
        foreach (Transform child in playerNamesContainer)
        {
            if (playerNames.ElementAtOrDefault<string>(i) != null)
            {
                child.GetComponentInChildren<TMP_InputField>().text = playerNames[i];
                i++;
            }
            else
            {
                return;
            }
        }
    }
}
