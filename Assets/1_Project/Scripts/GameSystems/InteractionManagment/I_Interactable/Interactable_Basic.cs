using Unity.Netcode;
using UnityEngine;

public class Interactable_Basic : MonoBehaviour, IInteractable
{
    public void TryInteract(NetworkObject interactionOwner = null)
    {
        Debug.Log($"TryInteract Callled on {gameObject.name}");
    }

    void Start()
    {
        var tempName = gameObject.name;
        gameObject.name = tempName + "_Interactable";
    }
}
