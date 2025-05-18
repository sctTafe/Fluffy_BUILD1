using UnityEngine;

public class PlayerAction_Debugger : PlayerActionBase
{
    public string _identifier;
    public override bool fn_ReceiveActivationInput(bool b)
    {
        Debug.Log($"PlayerAction_Debugger Called on {_identifier}!");
        return false;
    }
}
