using UnityEngine;
using Unity.Netcode;
using DG.Tweening.Core.Easing;
using UnityEngine.Events;

public class ScottsBackup_PlayerStealthMng : NetworkBehaviour
{
    private const bool ISDEBUGGING = true;

    public UnityEvent _OnPlayerEnterBush_Local;
    
    [SerializeField] GameObject _ChatacterVisuals;


    private bool in_bush = false;
    private float time_in_bush = 0;
    private float force_reveal = 0;

    
    HUD_PopUpMessages_Singelton _ref_hUDPopUpMessagesSingelton;



    void Start()
    {

        if (IsOwner)
        {
            _ref_hUDPopUpMessagesSingelton = HUD_PopUpMessages_Singelton.Instance;
        }

    }

    void Update()
    {
        if (in_bush)
        {
            time_in_bush += Time.deltaTime;

            if (time_in_bush > 0.8f && force_reveal <= 0)
            {
                if (!IsOwner)
                {
                    _ChatacterVisuals.SetActive(false);
                }
                else
                {
                    _OnPlayerEnterBush_Local?.Invoke();
                    _ref_hUDPopUpMessagesSingelton.fn_PopupMessage("[ Hidden! ]", HUD_PopUpMessages_Singelton.PopupStyle.PopAndFade);
                }
            }
        }

        force_reveal -= Time.deltaTime;

        // Doesn't show revealed text, probably needs to be an RPC
        if (IsOwner && force_reveal > 0)
        {
            _ref_hUDPopUpMessagesSingelton.fn_PopupMessage("[ Revealed! ]", HUD_PopUpMessages_Singelton.PopupStyle.PopAndFade);
        }

    }

    public void fn_SetInBush()
    {
        if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerStealthMng: fn_SetInBush Called");
        in_bush = true;
        time_in_bush = 0;

    }

    public void fn_SetLeavingBush()
    {
        if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerStealthMng: fn_SetLeavingBush Called");
        in_bush = false;

        if (!IsOwner)
        {
            _ChatacterVisuals.SetActive(true);
        }
        else
        {
            _ref_hUDPopUpMessagesSingelton.fn_PopupMessage("[ - - - ]", HUD_PopUpMessages_Singelton.PopupStyle.PopAndFade);
        }
    }


    // Forces the player to reveal for 10 seconds
    // Called with mutant scan attack

    // Reveal probably needs to be an RPC to get this to work
    public void force_unhide()
    {
        if (!IsOwner)
        {
            force_reveal = 10;
            _ChatacterVisuals.SetActive(true);
        }

        // force_reveal_ServerRPC();

    }

    [ServerRpc(RequireOwnership = false)]
    private void force_reveal_ServerRPC()
    {
        client_reveal_ClientRPC();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void client_reveal_ClientRPC()
    {
        if (IsOwner)
        {
            Debug.Log("Server RPC to reveal player was called");
            force_reveal = 10;
            _ref_hUDPopUpMessagesSingelton.fn_PopupMessage("[ Revealed! ]", HUD_PopUpMessages_Singelton.PopupStyle.PopAndFade);
        }
    }
}
