using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject wordRevealPanel;
    [SerializeField] private GameObject cluePanel;
    [SerializeField] private GameObject votingPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject votingResultPanel;

    private Player _currentPlayer;

    public static UIManager instance;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

    }

    private void Start()
    {
        Button wordRevealNxtPlayerBtn = wordRevealPanel.GetComponentInChildren<Button>();
        wordRevealNxtPlayerBtn.onClick.AddListener(() => { 
            if(RoundManager.instance.CurrentPlayerIndex == PlayerManager.instance.GetActivePlayers().Count - 2)
            {
                wordRevealNxtPlayerBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Start Clue Rounds";
            }

            RoundManager.instance.NextWordRevealPlayer();
            SetUpWordRevealPanel(); 
        });

        CluePanelUI cluePanelScript = cluePanel.GetComponent<CluePanelUI>();
        cluePanelScript.GetNextPlayerBtn.onClick.AddListener(() => {

            if (!GameData.isOnline || NetworkManager.Singleton.IsHost)
            {
                RoundManager.instance.NextPlayerClue();
            }
            else
            {
                RoundManager.instance.NextPlayerClueServerRpc();
            }
                SetUpCluePanel();
        });
        cluePanelScript.GetVotingTableBtn.onClick.AddListener(() => {
        cluePanelScript.OpenVotingTable();
        });

        VotingPanelUI votingPanelScript = votingPanel.GetComponent<VotingPanelUI>();
        Button btn = votingPanelScript.GetSkipBtn;
        btn.onClick.AddListener(() =>
        {
            Invoke("VoteBasedOnIsOnline", 1f);
        });
        

        RoundManager.instance.onPhaseChanged += HandlePhaseChanged;
        RoundManager.instance.onVoterChanged += HandleVoterChanged;
        VotingManager.instance.onPlayerEliminated += HandlePlayerEliminated;
    }

    private void OnDisable()
    {
        RoundManager.instance.onPhaseChanged -= HandlePhaseChanged;
        RoundManager.instance.onVoterChanged -= HandleVoterChanged;
        VotingManager.instance.onPlayerEliminated -= HandlePlayerEliminated;

    }

    public void HandlePlayerEliminated(Player player)
    {
        votingResultPanel.SetActive(true);

        ShowVotingResultPanel votingResultPanelScript = votingResultPanel.GetComponent<ShowVotingResultPanel>();
        if(player == null)
        {
            votingResultPanelScript.SetTieResult();
        }
        else
        {
            votingResultPanelScript.SetVotingPanelResult(player);
        }

        VotingPanelUI votingPanelScript = votingPanel.GetComponent<VotingPanelUI>();
        Button btn = votingPanelScript.GetSkipBtn;
        TextMeshProUGUI btnVoteCountTxtComp = btn.GetComponentInChildren<TextMeshProUGUI>();
        btnVoteCountTxtComp.text = "0";
        Invoke("DisableVotingResultPanel", 5f);
    }

    public void HandlePhaseChanged(RoundManager.GamePhase phase)
    {
        wordRevealPanel.SetActive(phase == RoundManager.GamePhase.WordReveal);
        cluePanel.SetActive(phase == RoundManager.GamePhase.Clue);
        votingPanel.SetActive(phase == RoundManager.GamePhase.Voting);
        resultPanel.SetActive(phase == RoundManager.GamePhase.Result);



        if(phase == RoundManager.GamePhase.WordReveal)
        {
            SetUpWordRevealPanel();
        }
        else if(phase == RoundManager.GamePhase.Clue)
        {
            SetUpCluePanel();
        }
        else if(phase == RoundManager.GamePhase.Voting)
        {
            SetUpVotingPanel();
        }
        else if(phase == RoundManager.GamePhase.Result)
        {
            ResultPanelUI resultPanelUIScript = resultPanel.GetComponent<ResultPanelUI>();

            StartCoroutine(resultPanelUIScript.ShowGameResult(RoundManager.instance.result));
            
        }
        Debug.Log("HandlePhaseChanged: " + phase);
    }

    public void HandleVoterChanged(int newPlayerIndex)
    {
        Debug.Log("HandleVoterChanged: newPlayerIndex=" + newPlayerIndex + " activeCount=" + PlayerManager.instance.GetActivePlayers().Count);

        if (newPlayerIndex >= PlayerManager.instance.GetActivePlayers().Count) return;

        Player player = PlayerManager.instance.GetActivePlayers()[newPlayerIndex];
        VotingPanelUI votingPanelUIScript = votingPanel.GetComponent<VotingPanelUI>();
        votingPanelUIScript.SetWhosTurn(player.name);
    }


    public void SetUpWordRevealPanel()
    {
        if (RoundManager.instance.CurrentPlayerIndex >= PlayerManager.instance.GetActivePlayers().Count) return;

        
        _currentPlayer = PlayerManager.instance.GetActivePlayers()[RoundManager.instance.CurrentPlayerIndex];
        CardFlip card = wordRevealPanel.transform.GetComponentInChildren<CardFlip>();
        Button nextPlayerBtn = wordRevealPanel.transform.GetComponentInChildren<Button>();

        //reset
        card.ResetCard();

        bool isImposter = GameData.isOnline ? NetworkPlayerManager.instance.isImposter : _currentPlayer.isImposter;
        string assignedWord = GameData.isOnline ? NetworkPlayerManager.instance.assignedWord : _currentPlayer.assignedWord;

        Debug.Log($"Wordreveal panel, isImposter:{isImposter} assignedWord:{assignedWord}");

        //set word
        if (GameData.canImposterHaveWord && isImposter)
        {
            card.SetWordText(assignedWord);
        }
        else
        {
            card.SetWordText("You have to blend in thourgh oppenent clues");
            if (!isImposter)
            {
                card.SetWordText(assignedWord);
            }
        }

        //set player type
        card.SetPlayerType(isImposter);


        //set name and nextPlayerBtn
        if (!GameData.isOnline)
        {
            card.SetNameText(_currentPlayer.name);
            nextPlayerBtn.gameObject.SetActive(true);
        }
        else
        {
            card.SetNameText("");
            nextPlayerBtn.gameObject.SetActive(false);
        }
    }


    public void SetUpCluePanel()
    {
        if (RoundManager.instance.CurrentPlayerIndex >= PlayerManager.instance.GetActivePlayers().Count) return;

        _currentPlayer = PlayerManager.instance.GetActivePlayers()[RoundManager.instance.CurrentPlayerIndex];

        CluePanelUI cluePanelScript = cluePanel.GetComponent<CluePanelUI>();

        cluePanelScript.SetUp(_currentPlayer.name, RoundManager.instance.CurrentRound);
    }


    public void SetUpVotingPanel()
    {
        VotingPanelUI votingPanelScript = votingPanel.GetComponent<VotingPanelUI>();
        votingPanelScript.DestroyAllVotingBtns();
        votingPanelScript.InstantiateVotingBtns(PlayerManager.instance.GetActivePlayers());

        bool isLocalPlayerEliminated = GameData.isOnline ? NetworkPlayerManager.instance.IsPlayerEliminated(GameData.devicePlayerName) : false;

        votingPanelScript.SetInteractable(!isLocalPlayerEliminated);

        if (isLocalPlayerEliminated)
        {
            votingPanelScript.SetSpectatorMode();
        }

        if (GameData.isOnline)
        {
            if (RoundManager.instance.CurrentPlayerIndex >= PlayerManager.instance.GetActivePlayers().Count) return;

            _currentPlayer = PlayerManager.instance.GetActivePlayers()[RoundManager.instance.CurrentPlayerIndex];
            votingPanelScript.SetWhosTurn("");
        }
        else
        {
            votingPanelScript.SetWhosTurn(_currentPlayer.name);
        }
    }


    public void DisableVotingResultPanel()
    {
        votingResultPanel.SetActive(false);
    }

    public void RestartGame()
    {
        GameData.ResetData();
        SceneManager.LoadScene(0);
    }

    public void VoteBasedOnIsOnline()
    {
        Player player = PlayerManager.instance.GetActivePlayers()[RoundManager.instance.CurrentPlayerIndex];

        if (GameData.isOnline)
        {
            VotingManager.instance.SkipVoteServerRpc(GameData.devicePlayerName);
        }
        else
        {
            VotingManager.instance.SkipVote(player);
        }
    }
}
