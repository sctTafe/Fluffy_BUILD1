using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_UnityEventTrigger : MonoBehaviour, IInteractable
{
    public UnityEvent OnTrigger;
    public void TryInteract(NetworkObject interactionOwner = null)
    {
        Debug.Log($"TryInteract Callled on {gameObject.name}");
        OnTrigger?.Invoke();
    }

    void Start()
    {
        var tempName = gameObject.name;
        gameObject.name = tempName + "_Interactable";
    }
}
