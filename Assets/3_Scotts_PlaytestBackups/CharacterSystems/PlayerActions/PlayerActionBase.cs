using Unity.Netcode;

/// <summary>
/// Implmented to allow the 'drag n drop' of the interace type
/// </summary>
public abstract class PlayerActionBase : NetworkBehaviour, IPlayerAction
{
    public abstract bool fn_ReceiveActivationInput(bool b);
}
