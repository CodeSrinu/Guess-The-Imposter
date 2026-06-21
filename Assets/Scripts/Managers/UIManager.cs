using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject wordRevealPanel;
    [SerializeField] private GameObject cluePanel;
    [SerializeField] private GameObject votingPanel;
    [SerializeField] private GameObject resultPanel;

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
        Button nxtPlayerBtn = wordRevealPanel.GetComponentInChildren<Button>();
        nxtPlayerBtn.onClick.AddListener(() => { 
            RoundManager.instance.NextWordRevealPlayer();
            SetUpWordRevealPanel(); 
        });

        CluePanelUI cluePanelScript = cluePanel.GetComponent<CluePanelUI>();
        cluePanelScript.GetNextPlayerBtn.onClick.AddListener(() => {
            RoundManager.instance.NextPlayerClue();
            SetUpCluePanel();
        });
        cluePanelScript.GetVotingTableBtn.onClick.AddListener(() => {
            cluePanelScript.OpenVotingTable();
        });

        VotingPanelUI votingPanelScript = votingPanel.GetComponent<VotingPanelUI>();
        votingPanelScript.GetSkipBtn.onClick.AddListener(() =>
        {
            VotingManager.instance.SkipVote(_currentPlayer);
        });
        



        RoundManager.instance.onPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        RoundManager.instance.onPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(RoundManager.GamePhase phase)
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
    }




    private void SetUpWordRevealPanel()
    {
        if (RoundManager.instance.CurrentPlayerIndex >= GameData.playersCount) return;

        
        _currentPlayer = PlayerManager.instance.GetPlayers[RoundManager.instance.CurrentPlayerIndex];
        CardFlip card = wordRevealPanel.transform.GetComponentInChildren<CardFlip>();
        Button nextPlayerBtn = wordRevealPanel.transform.GetComponentInChildren<Button>();

        //reset
        card.ResetCard();


        //set word
        if (GameData.canImposterHaveWord)
        {
            card.SetWordText(_currentPlayer.assignedWord);
        }
        else
        {
            card.SetWordText("You have to blend in thourgh oppenent clues");
        }

        //set player type
        card.SetPlayerType(_currentPlayer.isImposter);


        //set name and nextPlayerBtn
        if (!GameData.isOnline)
        {
            card.SetNameText(_currentPlayer.name);
            nextPlayerBtn.gameObject.SetActive(true);
        }
        else
        {
            nextPlayerBtn.gameObject.SetActive(false);
        }
    }


    private void SetUpCluePanel()
    {
        if (RoundManager.instance.CurrentPlayerIndex >= GameData.playersCount) return;

        _currentPlayer = PlayerManager.instance.GetPlayers[RoundManager.instance.CurrentPlayerIndex];

        CluePanelUI cluePanelScript = cluePanel.GetComponent<CluePanelUI>();

        cluePanelScript.SetPlayerName(_currentPlayer.name);
        cluePanelScript.SetCurrentRound(RoundManager.instance.CurrentRound);
    }


    private void SetUpVotingPanel()
    {
        if (RoundManager.instance.CurrentPlayerIndex >= GameData.playersCount) return;

        _currentPlayer = PlayerManager.instance.GetPlayers[RoundManager.instance.CurrentPlayerIndex];
        VotingPanelUI votingPanelScript = votingPanel.GetComponent<VotingPanelUI>();

        votingPanelScript.DestroyAllVotingBtns();
        votingPanelScript.InstantiateVotingBtns(VotingManager.instance.EligibleVoters, VotingManager.instance.EligiblePlayers);
        votingPanelScript.SetWhosTurn(_currentPlayer.name);
    }
}
