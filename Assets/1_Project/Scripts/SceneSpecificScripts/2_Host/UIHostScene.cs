using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIHostScene : Singleton<UIHostScene>
{
    [SerializeField] private Button _startHostButton;
    [SerializeField] private GameObject _startHostButtonGO;

    [SerializeField] private bool _isAutoStartEnabled;

    NetworkSceneManager _sceneManager;

    private void Awake()
    {
        Cursor.visible = true;
    }
    void Start()
    {
        _sceneManager = NetworkSceneManager.Instance;

        if (_isAutoStartEnabled)
        {
            _startHostButtonGO.SetActive(false);
            StartRelayAndHost();
        }
            
        // START HOST
        _startHostButton?.onClick.AddListener(() =>
        {
            Debug.Log("UIManager: StartHostBtn");
            StartRelayAndHost();
        });
    }

    private async void StartRelayAndHost()
    {
        Debug.Log("UIManager: Async StartRelayAndHost Called");
        // this allows the UnityMultiplayer and UnityMultiplayerRelay scene to work with and without
        // relay features - if the Unity transport is found and is relay protocol then we redirect all the 
        // traffic through the relay, else it just uses a LAN type (UNET) communication.
        if (RelayManager.Instance.IsRelayEnabled)
            await RelayManager.Instance.SetupRelay();

        if (NetworkManager.Singleton.StartHost())
        {
            _sceneManager?.fn_GoToScene("4_Lobby");
        }
    }
    //// Update is called once per frame
    //void Update()
    //{
        
    //}

}
