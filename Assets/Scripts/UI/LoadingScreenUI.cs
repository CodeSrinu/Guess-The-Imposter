using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject loadingPrefab;
    [SerializeField] private GameObject errorLoadingPrefab;
    [SerializeField] private Button errorPanelCloseBtn;

    private GameObject loadingScreenObj;
    private GameObject loadingErrorScreenObj;
    public static LoadingScreenUI instance;

    private void Awake()
    {

        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        loadingScreenObj = Instantiate(loadingPrefab, transform);
        loadingScreenObj.SetActive(false);

        loadingErrorScreenObj = Instantiate (errorLoadingPrefab, transform);
        loadingErrorScreenObj.SetActive(false);

        errorPanelCloseBtn = loadingErrorScreenObj.GetComponentInChildren<Button>();

        errorPanelCloseBtn.onClick.AddListener(() =>{
            loadingErrorScreenObj.SetActive(false);
        });
    }


    public void StartLoading()
    {
        if (loadingScreenObj == null)
        {
            Debug.Log("loadingprefab not assigned");
            return;
        }
        loadingErrorScreenObj.SetActive(false);
        loadingScreenObj.SetActive(true);

        Invoke("StopLoading", 30f);
    }

    public void StopLoading()
    {
        loadingScreenObj.SetActive(false);
    }


    public void ShowLoadingError(string error)
    {
        StopLoading();
        loadingErrorScreenObj.GetComponentInChildren<TextMeshProUGUI>().text = error;
        loadingErrorScreenObj?.SetActive(true);
    }

    public void CloseErrorPanel()
    {
        loadingErrorScreenObj?.SetActive(false);
    }
}
