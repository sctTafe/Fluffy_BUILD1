using UnityEngine;
using Unity.Netcode;

public interface IHoldable
{
    IHolder CurrentHolder { get; }

    /// <summary>
    /// Use: Pass the potential holder to this holdable object, if the is space it will retrieve parent and the related holding transform position
    /// Assignes a Holdable 
    /// </summary>
    /// <param name="parent"></param>
    public void fn_TryAssignNetworkObjectParentAndHoldingPosition(IHolder parent);

    // Fail Not Used?
    public void fn_AssignHolder(IHolder parent);

    public void fn_DeassignNetworkObjectParent();

    public void fn_DestoryHoldableNetworkObject();

    /// <summary>
    /// Sets the Parent Network Object transform of the holdable Item
    /// </summary>
    /// <param name="toBeParent"></param>
    public void fn_SetNetworkObjectParentToHolder(NetworkObject toBeParent);


    /// <summary>
    /// Returns the IHoldable Network Object
    /// </summary>
    /// <returns></returns>
    public NetworkObject fn_GetNetworkObject();

    public void fn_ClearHolder();




    /// <summary>
    /// Binds the Holdable Object to the Holder, this assigned the Parent Transform of the network object and get the Holding Position Transfrom
    /// </summary>
    void fn_BindToHolder(IHolder holder);

    /// <summary>
    /// Clears any Previouse Holder and transfrom parent  
    /// </summary>
    void fn_Drop();

    /// <summary>
    /// Destroys this network holdable items
    /// </summary>
    void fn_Destroy();

    /// <summary>
    /// Sets the Holdable Object to a specific Vector 3 Positon
    /// </summary>
    void fn_SetPosition(Vector3 position);

    /// <summary>
    /// Updates the holding position transform
    /// </summary>
    void fn_UpdateHoldingTransformPosition();

}
