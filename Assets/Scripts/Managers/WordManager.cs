using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

public class WordManager : MonoBehaviour
{
    private List<WordPair> _wordPairs;

    public static WordManager instance;

    public List<string> SelecetedCategories => selectedCategories;

    List<string> selectedCategories = new List<string>
    {
        "TOLLYWOOD",
        "TOLLYWOOD CELEBRITIES",
        "TELUGU MOVIE CHARACTERS",
        "ANDHRA/TELANGANA FOOD",
        "ANDHRA/TELANGANA PLACES",
        "TELUGU STATES CULTURE & DAILY LIFE",
        "TELUGU RELATIONS & SLANG",
        "SOUTH INDIAN CINEMA",
        "SOUTH INDIAN CELEBRITIES",
        "BOLLYWOOD",
        "BOLLYWOOD CELEBRITIES",
        "HOLLYWOOD",
        "FOREIGN CELEBRITIES",
        "WEB SERIES & OTT",
        "ANIME",
        "CRICKET",
        "SPORTS GENERAL",
        "INDIAN FOOD GENERAL",
        "TECHNOLOGY & GADGETS",
        "INDIAN MUSIC",
        "WESTERN MUSIC",
        "ANIMALS & NATURE",
        "PROFESSIONS",
        "PLACES IN INDIA",
        "FESTIVALS INDIA"
    };

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

        selectedCategories.Clear();
    }


    public WordPair GetRandomWordPair()
    {
        List<WordPair> filteredList = _wordPairs.Where(p => selectedCategories.Contains(p.category)).ToList();
        int randomIndex = Random.Range(0, filteredList.Count);

        return filteredList[randomIndex];

    }

    public void AddCategory(string category)
    {
        if (!selectedCategories.Contains(category))
        {
            selectedCategories.Add(category);
        }
    }

    public void RemoveCategory(string category)
    {
        if (selectedCategories.Contains(category))
        {
            selectedCategories.Remove(category);
        }
    }
}
