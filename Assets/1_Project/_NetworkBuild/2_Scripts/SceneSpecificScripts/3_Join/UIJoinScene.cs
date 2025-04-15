using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIJoinScene : Singleton<UIJoinScene>
{
    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private TMP_InputField joinCodeInput;


    NetworkSceneManager _sceneManager;
    private void Awake()
    {
        Cursor.visible = true;
    }
    void Start()
    {
        _sceneManager = NetworkSceneManager.Instance;
        // START CLIENT
        startClientButton?.onClick.AddListener(async () =>
        {
            Debug.Log("UIManager: startClientButton Called");
            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput.text))
                await RelayManager.Instance.JoinRelay(joinCodeInput.text);

            if (NetworkManager.Singleton.StartClient())
            {
                _sceneManager?.fn_GoToScene("4_Lobby");
            }
        });
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
