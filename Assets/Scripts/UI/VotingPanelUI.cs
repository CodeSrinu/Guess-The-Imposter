using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VotingPanelUI : MonoBehaviour
{
    [SerializeField] private Transform _votingGridContainer;
    [SerializeField] private TextMeshProUGUI voterNameTextComp;
    [SerializeField] private GameObject _votingBtnPrefab;
    [SerializeField] private Button _skipVoteBtn;
    private CanvasGroup _canvasGroup;

    public Button GetSkipBtn => _skipVoteBtn;
    public Transform GetVotingGridContainer => _votingGridContainer;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void DestroyAllVotingBtns()
    {
        foreach(Transform child in _votingGridContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetInteractable(bool interactable)
    {
        _canvasGroup.interactable = interactable;
        _canvasGroup.blocksRaycasts = interactable;
    }

    public void InstantiateVotingBtns(List<Player> players)
    {
        foreach (Player player in players)
        {
            GameObject btn = Instantiate(_votingBtnPrefab, _votingGridContainer);
            btn.GetComponent<VoteBtn>().SetUp(player);
        }
    }

    public void SetWhosTurn(string name)
    {
        voterNameTextComp.text = name + "\' turn to vote";
    }
    public void SetSpectatorMode()
    {
        voterNameTextComp.text = "Spectating...";
    }
}
