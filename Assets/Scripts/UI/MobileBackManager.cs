using UnityEngine;
using UnityEngine.SceneManagement;

public class MobileBackManager : MonoBehaviour
{

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackNavigation();
        }
    }

    void HandleBackNavigation()
    {

        int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;

        if (previousSceneIndex >= 0)
        {
            SceneManager.LoadScene(previousSceneIndex);
        }
        else
        {
            Application.Quit();
        }
    }
}