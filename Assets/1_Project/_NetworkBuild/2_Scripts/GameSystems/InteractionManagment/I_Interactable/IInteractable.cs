using Unity.Netcode;
using UnityEngine;

public interface IInteractable 
{
    public void TryInteract(NetworkObject interactionOwner = null);
}
