using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Use: IHolder for holding
/// IHoldable Network Object
/// Requirment: for object to be a Network Object
/// [RequireComponent(typeof(NetworkObject))]
/// </summary>

public interface IHolder 
{
    /// <summary>
    /// Checks if the IHolder has avalible holding space 
    /// </summary>
    /// <returns>
    /// Returns True if sapce is avalible & False if no space is avalible
    /// </returns>
    public bool fn_CheckIfHoldingSlotIsAvailable();

    /// <summary>
    /// 
    /// </summary>
    /// <returns>
    /// Returns IHolder if the is holding space avalible for the IHoldable, else Null if the IHolder has no spaces avalible 
    /// </returns>
    public IHolder fn_AssignNextAvailableHoldingPosition(IHoldable holdableNetworkObject);
    
    
    /// <summary>
    /// Returns the Transform where the holdable object shall appear on the Holder
    /// </summary>
    public Transform fn_GetTransfromOfHoldingPosition(IHoldable holdableNetworkObject);


    /// <summary>
    /// Returns the IHolders Network Object
    /// </summary>
    public NetworkObject fn_GetNetworkObject();

    /// <summary>
    /// Drops Item from Slot 0 Holdable Item
    /// </summary>
    public void fn_DropSlot0Item();

    /// <summary>
    /// Destroys and Despawn Slot 0 Holdable Item
    /// </summary>
    public void fn_DestroySlot0Item();

    /// <summary>
    /// Cycles the Holdable Items through the Holders Avalible slots 
    /// </summary>
    public void fn_CycleItemSlots();
}
