using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFowarding : MonoBehaviour
{
    public enum SceneStates
    {
        error,
        Host,
        Client
    }

    public SceneStates _sceneTarget;

    // Buttons
    [SerializeField]
    private Button _startHost;
    [SerializeField]
    private Button _startClient;

    // Scene Names
    [SerializeField]
    private string _hostScene;
    // Scene Names
    [SerializeField]
    private string _clientScene;
    [SerializeField]
    private string _bootstrap;


    void Start()
    {
        if (_startHost != null)
            _startHost?.onClick.AddListener(() =>
            {
                fn_StartHost();
            });

        if (_startClient != null)
            _startClient?.onClick.AddListener(() =>
            {
                fn_StartClient();
            });
        DontDestroyOnLoad(gameObject);
    }

    public void fn_StartHost()
    {
        _sceneTarget = SceneStates.Host;
        AsyncOperation asyncOp  = SceneManager.LoadSceneAsync(_bootstrap);
        asyncOp.completed += OnSceneLoaded;
    }

    public void fn_StartClient()
    {
        _sceneTarget = SceneStates.Client;
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(_bootstrap);
        asyncOp.completed += OnSceneLoaded;
    }

    private void OnSceneLoaded(AsyncOperation operation)
    {
        if (_sceneTarget == SceneStates.Host)
        {
            NetworkSceneManager.Instance.fn_GoToScene(_hostScene);
        }
        else
        {
            NetworkSceneManager.Instance.fn_GoToScene(_clientScene);
        }

        Destroy(this.gameObject);
    }



    
}
