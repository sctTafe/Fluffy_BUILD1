using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Inefficent way to do it but is currently providing an easy way to check the inputs are doing as expected 
/// </summary>
public class ScottsBackup_ActionMappingSystem : NetworkBehaviour
{
    ScottsBackupInputSystem playerInputs;

    [SerializeReference] public PlayerActionBase _Action_Main;
    [SerializeReference] public PlayerActionBase _Action_Sprint;
    [SerializeReference] public PlayerActionBase _Action_Jump;
    [SerializeReference] public PlayerActionBase _Action_Interaction1;
    [SerializeReference] public PlayerActionBase _Action_Interaction2;
    [SerializeReference] public PlayerActionBase _Action_Interaction3;
    [SerializeReference] public PlayerActionBase _Action_Interaction4;

    private void Start()
    {
        playerInputs = ScottsBackup_InputRefSingleton.Instance._inputs;

        // Disable Self If Not Owner
        if(!IsOwner)
            this.enabled = false;
    }

    private void Update()
    {
        if (playerInputs.sprint == true)
        {
            // Is a pass through value, dont need to clear it
            if(_Action_Sprint != null)
                _Action_Sprint.fn_ReceiveActivationInput(true);
        }

        if (playerInputs.jump == true)
        {
            if (_Action_Jump != null)
                playerInputs.jump = _Action_Jump.fn_ReceiveActivationInput(true);
        }

        if (playerInputs.attack == true)
        {
            if (_Action_Main != null)
                playerInputs.attack = _Action_Main.fn_ReceiveActivationInput(true);
        }

        if (playerInputs.interaction1 == true)
        {
            if (_Action_Interaction1 != null)
                playerInputs.interaction1 = _Action_Interaction1.fn_ReceiveActivationInput(true);
        }

        if (playerInputs.interaction2 == true)
        {
            if (_Action_Interaction2 != null)
                playerInputs.interaction2 = _Action_Interaction2.fn_ReceiveActivationInput(true);
        }

        if (playerInputs.interaction3 == true)
        {
            if (_Action_Interaction3 != null)
                playerInputs.interaction3 = _Action_Interaction3.fn_ReceiveActivationInput(true);
        }

        if (playerInputs.interaction4 == true)
        {
            if (_Action_Interaction4 != null)
                playerInputs.interaction4 = _Action_Interaction4.fn_ReceiveActivationInput(true);
        }

    }

}
