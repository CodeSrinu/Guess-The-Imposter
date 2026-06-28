using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

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
        
        string[] targetCategoriesForNow = { "TOLLYWOOD",
                                            "TOLLYWOOD CELEBRITIES",
                                            "TELUGU MOVIE CHARACTERS",
                                            "ANDHRA/TELANGANA FOOD",
                                            "ANDHRA/TELANGANA PLACES",
                                            "TELUGU STATES CULTURE & DAILY LIFE",
                                            "TELUGU RELATIONS & SLANG",
                                            "TECHNOLOGY & GADGETS",
                                            "ANIMALS & NATURE",
                                            "PROFESSIONS"
                                            };
        List<WordPair> filteredList = _wordPairs.Where(p => targetCategoriesForNow.Contains(p.category)).ToList();
        int randomIndex = Random.Range(0, filteredList.Count);

        return filteredList[randomIndex];

    }


}
