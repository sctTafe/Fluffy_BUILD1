using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerTeam : MonoBehaviour
{
    [SerializeField] private TMP_Text currentTeam_txt;
    [SerializeField] private TMP_Text mutantsTotal_txt;
    [SerializeField] private Button switchTeam_btn;

    PlayerNetworkDataManager playerNetworkDataManager;

    #region Unity Native Functions
    void Start()
    {
        playerNetworkDataManager = PlayerNetworkDataManager.Instance;
        playerNetworkDataManager.OnPlayerDataNetworkListChanged += Handle_OnPlayerDataChanged;
        playerNetworkDataManager.OnTeamsChanged += Handle_OnTeamsChanged;
        
        // Guard
        if(playerNetworkDataManager == null) Debug.LogError("UIPlayerTeam Trying to Start Before PlayerNetworkDataManager");
        

        // Team Chnage Button
        if (switchTeam_btn != null)
        {
            switchTeam_btn.onClick.AddListener(() => {
                playerNetworkDataManager.fn_SwitchTeamToggle();
            });
        }

        UpdateUIValues();

    }



    private void OnDestroy()
    {
        playerNetworkDataManager.OnTeamsChanged -= Handle_OnTeamsChanged;
        playerNetworkDataManager.OnTeamsChanged -= Handle_OnPlayerDataChanged;
    }
    #endregion END: Unity Native Functions

    private void Handle_OnTeamsChanged(object sender, EventArgs e)
    {
        UpdateUIValues();
    }
    private void Handle_OnPlayerDataChanged(object sender, EventArgs e)
    {
        UpdateUIValues();
    }


    private void UpdateUIValues()
    {
        currentTeam_txt.text = playerNetworkDataManager.fn_GetLocalClinetTeaam() ? "Fluffy" : "Mutant";
        mutantsTotal_txt.text = playerNetworkDataManager.fn_GetTotalMutantPlayers().ToString();
    }
}
