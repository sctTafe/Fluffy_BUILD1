using UnityEngine;

public class HUD_PopUpIcon_Caller : MonoBehaviour
{
    public void fn_CallPopupIcon()
    {
        if(HUD_PopUpIcon_Singleton.Instance != null)
        {
            HUD_PopUpIcon_Singleton.Instance.fn_PopupIcon(HUD_PopUpIcon_Singleton.PopupStyle.Bounce, 2f);
        }
        else
        {
            Debug.LogError("HUD_PopUpIcon_Caller could not find 'HUD_PopUpIcon_Singleton.Instance' plz fix!");
        }       
    }

    public void fn_CancelPopupIcon()
    {
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
