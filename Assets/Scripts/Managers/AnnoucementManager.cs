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

    private void Start()
    {
        _pool = new GameObject[poolSize];
        announcementsContainer = GetComponent<Transform>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateAnnoucementItem();
        }
    }

    private GameObject CreateAnnoucementItem()
    {
        GameObject annoucementItem = Instantiate(annoucementPrefab, announcementsContainer);
        annoucementItem.SetActive(false);
        _pool.Append(annoucementItem);
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
        GameObject annoucementItem = GetFreeAnnoucementItem();
        annoucementItem.GetComponent<TextMeshProUGUI>().text = annoucementTxt;
        annoucementItem.SetActive(true);
        StartCoroutine(RemoveAnnoucement(annoucementItem));
    }

    private IEnumerator RemoveAnnoucement(GameObject annoucementItem)
    {
        yield return new WaitForSeconds(2f);
        annoucementItem.SetActive(false);
    }
}
