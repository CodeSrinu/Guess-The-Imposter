using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        Button nxtPlayerBtn = wordRevealPanel.GetComponentInChildren<Button>();
        nxtPlayerBtn.onClick.AddListener(() => { 
            if(RoundManager.instance.CurrentPlayerIndex == PlayerManager.instance.GetActivePlayers().Count - 2)
            {
                nxtPlayerBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Start Clue Rounds";
            }

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
        Button btn = votingPanelScript.GetSkipBtn;
        TextMeshProUGUI btnVoteCountTxtComp = btn.GetComponentInChildren<TextMeshProUGUI>();
        btn.onClick.AddListener(() =>
        {
            int voteCount;
            if (!int.TryParse(btnVoteCountTxtComp.text, out voteCount))
            {
                voteCount = 0;
            }
            voteCount++;
            btnVoteCountTxtComp.text = voteCount.ToString();
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
        Invoke("DisableVotingResultPanel", 3f);
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
        else if(phase == RoundManager.GamePhase.Result)
        {
            ResultPanelUI resultPanelUIScript = resultPanel.GetComponent<ResultPanelUI>();

            StartCoroutine(resultPanelUIScript.ShowGameResult(RoundManager.instance.result));
            
        }
        Debug.Log("HandlePhaseChanged: " + phase);
    }

    private void HandleVoterChanged(int newPlayerIndex)
    {
        Debug.Log("HandleVoterChanged: newPlayerIndex=" + newPlayerIndex + " activeCount=" + PlayerManager.instance.GetActivePlayers().Count);

        if (newPlayerIndex >= PlayerManager.instance.GetActivePlayers().Count) return;

        Player player = PlayerManager.instance.GetActivePlayers()[newPlayerIndex];
        VotingPanelUI votingPanelUIScript = votingPanel.GetComponent<VotingPanelUI>();
        votingPanelUIScript.SetWhosTurn(player.name);
    }


    private void SetUpWordRevealPanel()
    {
        if (RoundManager.instance.CurrentPlayerIndex >= PlayerManager.instance.GetActivePlayers().Count) return;

        
        _currentPlayer = PlayerManager.instance.GetActivePlayers()[RoundManager.instance.CurrentPlayerIndex];
        CardFlip card = wordRevealPanel.transform.GetComponentInChildren<CardFlip>();
        Button nextPlayerBtn = wordRevealPanel.transform.GetComponentInChildren<Button>();

        //reset
        card.ResetCard();

        bool isImposter = GameData.isOnline ? NetworkPlayerManager.instance.isImposter : _currentPlayer.isImposter;
        string assignedWord = GameData.isOnline ? NetworkPlayerManager.instance.assignedWord : _currentPlayer.assignedWord;

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
            nextPlayerBtn.gameObject.SetActive(false);
        }
    }


    private void SetUpCluePanel()
    {
        if (RoundManager.instance.CurrentPlayerIndex >= PlayerManager.instance.GetActivePlayers().Count) return;

        _currentPlayer = PlayerManager.instance.GetActivePlayers()[RoundManager.instance.CurrentPlayerIndex];

        CluePanelUI cluePanelScript = cluePanel.GetComponent<CluePanelUI>();

        cluePanelScript.SetPlayerName(_currentPlayer.name);
        cluePanelScript.SetCurrentRound(RoundManager.instance.CurrentRound);
    }


    private void SetUpVotingPanel()
    {
        if (RoundManager.instance.CurrentPlayerIndex >= PlayerManager.instance.GetActivePlayers().Count) return;

        _currentPlayer = PlayerManager.instance.GetActivePlayers()[RoundManager.instance.CurrentPlayerIndex];
        VotingPanelUI votingPanelScript = votingPanel.GetComponent<VotingPanelUI>();

        votingPanelScript.DestroyAllVotingBtns();
        votingPanelScript.InstantiateVotingBtns(PlayerManager.instance.GetActivePlayers());
        votingPanelScript.SetWhosTurn(_currentPlayer.name);
    }


    public void DisableVotingResultPanel()
    {
        votingResultPanel.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    public void VoteBasedOnIsOnline()
    {
        if (GameData.isOnline)
        {
            VotingManager.instance.SkipVoteServerRpc(_currentPlayer.name);
        }
        else
        {
            VotingManager.instance.SkipVote(_currentPlayer);
        }
    }
}
