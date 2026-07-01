using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryPanelUI : MonoBehaviour
{
    [SerializeField] private Transform categoryBtnsContainer;
    [SerializeField] private GameObject categoryBtnPrefab;
    [SerializeField] private Button backBtn;
    public List<string> allCategories = new List<string> 
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

    public Button BackBtn => backBtn;


    public void PopulateCategories()
    {
        string savedString = PlayerPrefs.GetString("CategoriesTemplate", "");
        List<string> savedSelectedCategories = savedString.Split(",").Where(s => s != "").ToList();

        bool hasSavedData = savedSelectedCategories.Count > 0;

        foreach (var category in allCategories)
        {
            GameObject obj = Instantiate(categoryBtnPrefab, categoryBtnsContainer);
            CategoryBtn btnScript = obj.GetComponent<CategoryBtn>();
            btnScript.categoryName = category;

            btnScript.isSelected = hasSavedData ? savedSelectedCategories.Contains(category) : true;

            if (btnScript.isSelected)
                WordManager.instance.AddCategory(category);

            btnScript.ChangeTheBtnStatus(btnScript.isSelected);

            obj.GetComponentInChildren<TextMeshProUGUI>().text = category;
        }
    }

    public int GetTheCountOfCategoriesSelected()
    {
        int count = 0;
        foreach(Transform categoryBtn in categoryBtnsContainer)
        {
            if (categoryBtn.gameObject.GetComponent<CategoryBtn>().isSelected)
            {
                count++;
            }
        }

        return count;
    }
}
