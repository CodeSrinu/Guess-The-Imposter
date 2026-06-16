using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class WordManager : MonoBehaviour
{
    private List<WordPair> _wordPairs;

    public static WordManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        instance = this;


        string wordsData = Resources.Load<TextAsset>("wordsData").text;
        _wordPairs = JsonConvert.DeserializeObject<List<WordPair>>(wordsData);

    }


    public WordPair GetRandomWordPair()
    {
        int randomIndex = Random.Range(0, _wordPairs.Count);
        return _wordPairs[randomIndex];
    }


}
