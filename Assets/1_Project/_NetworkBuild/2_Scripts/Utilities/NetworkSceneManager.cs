using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkSceneManager : Singleton<NetworkSceneManager>
{
    // Buttons
    [SerializeField]
    private Button _mainMenuButton;
    [SerializeField]
    private Button _nextSceneButton;
    [SerializeField]
    private Button _quitButton;


    void Start()
    {
        if (_mainMenuButton != null)
            _mainMenuButton?.onClick.AddListener(() =>
            {
                fn_GoToMainMenu();
            });

        if (_nextSceneButton != null)
            _nextSceneButton?.onClick.AddListener(() =>
            {
                fn_SceneSwitch_NextScene();
            });


        if (_quitButton != null)
            _quitButton?.onClick.AddListener(() =>
            {
                Debug.Log("Menu_UIMng: Quit Btn Called, Quitting Application");
                Application.Quit();
            });
    }

    private void OnDestroy()
    {
        if (_mainMenuButton != null)
            _mainMenuButton.onClick.RemoveAllListeners();

        if (_nextSceneButton != null)
            _nextSceneButton.onClick.RemoveAllListeners();

        if (_quitButton != null)
            _quitButton.onClick.RemoveAllListeners();
    }

    string SceneName(int buildIndex)
    {
        string nextSceneName = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        return System.IO.Path.GetFileNameWithoutExtension(nextSceneName);
    }

    public void fn_GoToScene(string scene)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            
            Debug.Log($"Try Go To Scene: '{scene}'");
            NetworkManager.Singleton.SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("NetworkSceneManager: Network Not Established; Using Basic ScenManagere to switch scene!");
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);
        }
 
    }

    public void fn_GoToMainMenu()
    {
        Debug.Log("Home Scene Btn Called, loading next scene");
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.SceneManager.LoadScene(SceneName(0), LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("NetworkSceneManager: Network Not Established; Using Basic ScenManagere to switch scene!");
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
        }
    }

    public void fn_SceneSwitch_NextScene()
    {
        int nextSceneID = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneID < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("SceneManager_Netcode: 'Next Scene' Called, loading next scene");
            NetworkManager.Singleton.SceneManager.LoadScene(SceneName(nextSceneID), LoadSceneMode.Single);
        }
        else 
        {
            fn_GoToMainMenu();
        }

    }
}
