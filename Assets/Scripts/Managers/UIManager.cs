using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject wordRevealPanel;
    [SerializeField] private GameObject cluePanel;
    [SerializeField] private GameObject votingPanel;
    [SerializeField] private GameObject resultPanel;


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
    }
}
