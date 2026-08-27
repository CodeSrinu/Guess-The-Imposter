using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class AnnoucementManager : MonoBehaviour
{
    private int poolSize = 10;
    private GameObject[] _pool;
    [SerializeField] private GameObject annoucementPrefab;
    private Transform announcementsContainer;
    private int _poolIndex = 0;

    private void Start()
    {
        _pool = new GameObject[poolSize + 5];
        announcementsContainer = GetComponent<Transform>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateAnnoucementItem();
        }
    }

    private GameObject CreateAnnoucementItem()
    {
        if(_poolIndex >= poolSize) return null;

        GameObject annoucementItem = Instantiate(annoucementPrefab, announcementsContainer);
        annoucementItem.SetActive(false);
        _pool[_poolIndex] = annoucementItem;
        _poolIndex++;
        return annoucementItem;
    }

    private GameObject GetFreeAnnoucementItem()
    {
        foreach(GameObject item in _pool)
        {
            if (!item.activeSelf) return item;
        }
        return CreateAnnoucementItem();
    }

    public void GiveAnnoucement(string annoucementTxt)
    {
        Debug.Log($"AnnoucementManager: GiveAnnoucement is triggered with text:{annoucementTxt}");
        GameObject annoucementItem = GetFreeAnnoucementItem();
        annoucementItem.GetComponent<TextMeshProUGUI>().text = annoucementTxt;
        annoucementItem.SetActive(true);
        StartCoroutine(RemoveAnnoucement(annoucementItem));
    }

    private IEnumerator RemoveAnnoucement(GameObject annoucementItem)
    {
        Debug.Log($"AnnoucementManager: RemoveAnnoucement is triggered with item:{annoucementItem.GetComponent<TextMeshProUGUI>().text}");
        yield return new WaitForSeconds(2f);
        annoucementItem.SetActive(false);
    }
}
