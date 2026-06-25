using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VoteBtn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _voterBtnTextComp;
    [SerializeField] private TextMeshProUGUI _castedVotesTextComp;
    private Player _player;

    public Player GetPlayer => _player;

    private void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (GameData.isOnline)
            {
                VotingManager.instance.CastVoteServerRpc(_player.name);
            }
            else
            {
                int count = VotingManager.instance.CastVote(_player);
                UpdateVoteCount(count);
            }

            RoundManager.instance.NextVoter();

        });
    }


    public void SetUp(Player player)
    {
        _player = player;
        _voterBtnTextComp.text = _player.name;
    }

    public void UpdateVoteCount(int count)
    {
        _castedVotesTextComp.text = count.ToString();
    }



}
