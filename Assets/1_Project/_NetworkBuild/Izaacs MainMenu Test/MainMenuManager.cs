using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;

public class MainMenuManager : MonoBehaviour
{
    public enum MenuState { MainMenu, Lobby, Join }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private GameObject hostManager;
    [SerializeField] private GameObject hostPanel;
    [SerializeField] private GameObject hostCodePanel;
    [SerializeField] private GameObject lobbyPanel;

    [Header("UI Buttons")]
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button readyHostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button joinSubmitButton;
    [SerializeField] private Button backButton;

    [Header("Join UI (TextMeshPro)")]
    [SerializeField] private TMP_InputField joinCodeInput;

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera camMainMenu;
    [SerializeField] private CinemachineCamera camHost;
    [SerializeField] private CinemachineCamera camLobby;

    [Header("Priority Settings")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 10;

    private MenuState currentState = MenuState.MainMenu;

    void Start()
    {
        // Initialize UI state.
        mainMenuPanel.SetActive(true);
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(false);

        SetActiveCamera(camMainMenu);

        // Assign button callbacks.
        startHostButton.onClick.AddListener(OnHostButton);
        readyHostButton.onClick.AddListener(() => OnReadyHostButton());
        joinButton.onClick.AddListener(OnJoinButton);
        joinSubmitButton.onClick.AddListener(OnJoinSubmit);
        backButton.onClick.AddListener(OnBackButton);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState != MenuState.MainMenu)
            {
                SetMenuState(MenuState.MainMenu);
                mainMenuPanel.SetActive(true);
                joinPanel.SetActive(false);
                lobbyPanel.SetActive(false);
            }
        }
    }

    void OnHostButton()
    {
        //if (NetworkManager.Singleton.StartHost())
        {
            mainMenuPanel.SetActive(false);
            joinPanel.SetActive(false);
            hostManager.SetActive(true);
            hostPanel.SetActive(true);
            SetMenuState(MenuState.Lobby);
            Invoke(nameof(TransitionToLobby), 1.0f);
        }
    }

    async void OnReadyHostButton()
    {
        Debug.Log("Starting Host...");

        readyHostButton.gameObject.SetActive(false);

        if (RelayManager.Instance.IsRelayEnabled)
        {
            await RelayManager.Instance.SetupRelay();
        }

        if (RelayManager.Instance == null)
        {
            Debug.LogError("RelayManager.Instance is null!");
            return;
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager is null!");
            return;
        }

        if (NetworkManager.Singleton.GetComponent<UnityTransport>() == null)
        {
            Debug.LogError("UnityTransport is missing from NetworkManager!");
            return;
        }

        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host started successfully!");
            hostCodePanel.SetActive(true);
            lobbyPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Failed to start host.");
        }
    }

    void OnJoinButton()
    {
        joinPanel.SetActive(true);
        SetMenuState(MenuState.Join);
    }

    void OnJoinSubmit()
    {
        string code = joinCodeInput.text;
        if (!string.IsNullOrEmpty(code))
        {
            Debug.Log("Joining with code: " + code);
            joinPanel.SetActive(false);
            mainMenuPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            SetMenuState(MenuState.Lobby);
        }
    }

    void OnBackButton()
    {
        SetMenuState(MenuState.MainMenu);
        mainMenuPanel.SetActive(true);
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(false);
    }

    void TransitionToLobby()
    {
        SetMenuState(MenuState.Lobby);
    }

    void SetMenuState(MenuState state)
    {
        currentState = state;
        switch (state)
        {
            case MenuState.MainMenu:
                SetActiveCamera(camMainMenu);
                break;
            case MenuState.Lobby:
                SetActiveCamera(camLobby);
                break;
            case MenuState.Join:
                // For Join, you might opt to keep the Main Menu camera.
                SetActiveCamera(camMainMenu);
                break;
        }
    }

    void SetActiveCamera(CinemachineCamera activeCam)
    {
        // Set the active camera's priority higher than the others.
        camMainMenu.Priority = (activeCam == camMainMenu) ? activePriority : inactivePriority;
        camHost.Priority = (activeCam == camHost) ? activePriority : inactivePriority;
        camLobby.Priority = (activeCam == camLobby) ? activePriority : inactivePriority;
    }
}
