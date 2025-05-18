using UnityEngine;

public class HUD_PopUpMessages_Test : MonoBehaviour
{

    public HUD_PopUpMessages_Singelton popup;

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
            popup.fn_PopupMessage("Bounce Message", HUD_PopUpMessages_Singelton.PopupStyle.Bounce, 2f);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            popup.fn_PopupMessage("Pop & Fade Message", HUD_PopUpMessages_Singelton.PopupStyle.PopAndFade, 3f);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            popup.fn_PopupMessage("Wobble Message", HUD_PopUpMessages_Singelton.PopupStyle.Wobble, 1.5f);
    }
}
