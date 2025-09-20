using UnityEngine;

public class OnTigger_LocalPlayer_HUDPopUpIcon : MonoBehaviour
{
    const bool DEBUGGING = false;

    public string tagToDetect = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagToDetect))
        {
            if (DEBUGGING) Debug.Log($"OnTigger_LocalPlayer_HUDPopUpIcon called on {this.gameObject.name}");
            if (other.TryGetComponent<HUD_PopUpIcon_Caller_LocalNetworkPlayerOnly>(out var pUIC))
            {

                if (DEBUGGING) Debug.Log($"HUD_PopUpIcon_Caller_LocalNetworkPlayerOnly found on {pUIC.gameObject.name}");
                pUIC.fn_CallPopupIcon_OnLocalPlayer();
                
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagToDetect))
        {
            if (other.TryGetComponent<HUD_PopUpIcon_Caller_LocalNetworkPlayerOnly>(out var pUIC))
            {

                pUIC.fn_CancelPopupIcon_OnLocalPlayer();
                
            }
        }
    }
}
