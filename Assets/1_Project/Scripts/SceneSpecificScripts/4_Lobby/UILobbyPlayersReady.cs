using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyPlayersReady : MonoBehaviour
{
    [SerializeField] private TMP_Text playersReady_txt;
    [SerializeField] private TMP_Text playersTotal_txt;
    [SerializeField] private Button ready_btn;
    [SerializeField] private Button start_btn;

    LobbyManager preGameLobbyManager;
    void Start()
    {
        preGameLobbyManager = LobbyManager.Instance;
        preGameLobbyManager.OnReadyChanged += Handle_OnReadyChanged;

        // Toggle Ready
        if (ready_btn != null)
        {
            ready_btn.onClick.AddListener(() => {
                preGameLobbyManager.fn_PlayerReadyToggle();
            });
        }

        // Start Game
        if (start_btn != null)
        {
            start_btn.onClick.AddListener(() => {
                preGameLobbyManager.fn_StartGame();
            });
        }


        UpdatePlayerRdyValues();
    }
    private void OnDestroy()
    {
        preGameLobbyManager.OnReadyChanged -= Handle_OnReadyChanged;
    }

    private void Handle_OnReadyChanged(object sender, EventArgs e)
    {
        UpdatePlayerRdyValues();
    }

    private void UpdatePlayerRdyValues()
    {
        playersReady_txt.text = preGameLobbyManager.fn_GetNumberOfReadyPlayersInLobby().ToString();
        playersTotal_txt.text = preGameLobbyManager.fn_GetNumberOfPlayersInLobby().ToString();

    }
}
