using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkSceneManager : Singleton<NetworkSceneManager>
{
    // Scene Names
    [SerializeField]
    private string _hostScene = "2_Host";
    [SerializeField]
    private string _clientScene = "3_Join";
    [SerializeField]
    private string _bootstrap = "0_BootStrap";


    // Buttons
    [SerializeField]
    private Button _mainMenuButton;
    [SerializeField]
    private Button _nextSceneButton;
    [SerializeField]
    private Button _quitButton;
    [SerializeField]
    private Button _shutdownNetworkButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy any duplicate
            return;
        }
    }

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
                fn_QuitGame();
            });

        if (_shutdownNetworkButton != null)
            _shutdownNetworkButton?.onClick.AddListener(() =>
            {
                fn_Disconnect();
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

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= Handle_OnClientDisconnected;
    }

    private void Handle_OnClientDisconnected(ulong clientId)
    {
        Debug.Log("Handle_OnClientDisconnected Called!");
        fn_GoToMainMenu();
    }

    string SceneName(int buildIndex)
    {
        string nextSceneName = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        return System.IO.Path.GetFileNameWithoutExtension(nextSceneName);
    }
    public void fn_QuitGame()
    {
        Debug.Log("Menu_UIMng: Quit Btn Called, Quitting Application");
        Application.Quit();
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
            NetworkManager.Singleton.SceneManager.LoadScene(SceneName(1), LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("NetworkSceneManager: Network Not Established; Using Basic ScenManagere to switch scene!");
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
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

    /// <summary>
    /// 
    /// </summary>
    public void fn_Disconnect()
    {
        Debug.LogWarning("NetworkSceneManager: Disconnect Called!");

        if (PlayerNetworkDataManager.Instance != null)
            PlayerNetworkDataManager.Instance.fn_ClearPlayerDataManager();

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        fn_GoToMainMenu();
    }


    public void fn_Disconnect_ToMainMenu()
    {
        Debug.LogWarning("NetworkSceneManager: Disconnect Called!");

        //Force Scene
        SceneManager.LoadSceneAsync(1);

        if (PlayerNetworkDataManager.Instance != null)
            PlayerNetworkDataManager.Instance.fn_ClearPlayerDataManager();

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        
        //Debug.Log("fn_Disconnect_ToMainMenu -> RequestReturnToMenuServerRpc Called");
        //RequestReturnToMenuServerRpc();
    }



    public void fn_StartHost() 
    { 
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(_hostScene);
    }

    public void fn_StartClient()
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(_clientScene);
    }




    [ServerRpc]
    private void RequestReturnToMenuServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("RequestReturnToMenuServerRpc Caslled");
        // Only server/host can load scene
        NetworkManager.Singleton.SceneManager.LoadScene(SceneName(1), LoadSceneMode.Single);
    }
}
