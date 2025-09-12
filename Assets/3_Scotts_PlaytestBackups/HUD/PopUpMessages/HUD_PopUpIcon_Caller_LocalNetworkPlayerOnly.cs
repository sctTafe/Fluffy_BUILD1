using Unity.Netcode;
using UnityEngine;

public class HUD_PopUpIcon_Caller_LocalNetworkPlayerOnly : NetworkBehaviour 
{
    const bool DEBUGGING = false;

    private bool isEnabled = false;

    void Start()
    {
        //Disable this script unless it on the owner
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }
            
        isEnabled = true;
    }


    public void fn_CallPopupIcon_OnLocalPlayer()
    {
        // Function can be called on disabled scripts, which we dont want, so we check if this scirpt is enabled as a gaurd
        if (!isEnabled)
            return;

        if (DEBUGGING) Debug.Log($"fn_CallPopupIcon_OnLocalPlayer called on {this.gameObject.name}");
        if (HUD_PopUpIcon_Singleton.Instance != null)
        {
            HUD_PopUpIcon_Singleton.Instance.fn_PopupIcon(HUD_PopUpIcon_Singleton.PopupStyle.Bounce, 2f);
        }
        else
        {
            Debug.LogError("HUD_PopUpIcon_Caller could not find 'HUD_PopUpIcon_Singleton.Instance' plz fix!");
        }
    }

    public void fn_CancelPopupIcon_OnLocalPlayer()
    {
        // Function can be called on disabled scripts, which we dont want, so we check if this scirpt is enabled as a gaurd
        if (!isEnabled)
            return;

        if (DEBUGGING) Debug.Log($"fn_CancelPopupIcon_OnLocalPlayer called on {this.gameObject.name}");
        if (HUD_PopUpIcon_Singleton.Instance != null)
        {
            HUD_PopUpIcon_Singleton.Instance.fn_CancelPopup();
        }
        else
        {
            Debug.LogError("HUD_PopUpIcon_Caller could not find 'HUD_PopUpIcon_Singleton.Instance' plz fix!");
        }
    }

}
