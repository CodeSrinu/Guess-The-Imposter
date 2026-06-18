using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VotingManager : MonoBehaviour
{
    public static VotingManager instance;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void TallyVotes()
    {

    }
}
