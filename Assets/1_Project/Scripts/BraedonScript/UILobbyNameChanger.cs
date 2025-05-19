using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyNameChanger : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerName_txt;
    [SerializeField] private Button Name_btn;
    
    void Start()
    {
        // Toggle Ready
        if (Name_btn != null)
        {
            Name_btn.onClick.AddListener(() => {
                ChangeName();
            });
        }


    }
    private void OnDestroy()
    {
        
    }

    private void ChangeName()
    {
        PlayerNetworkDataManager player = PlayerNetworkDataManager.Instance;
        if (player != null) player.SetPlayerName(playerName_txt.text);
    }

    
}
