using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 
/// Dependant On:
///     GameItemsManager
/// </summary>
public class Interactable_SpawnIHoldableItem : MonoBehaviour, IInteractable
{
    [SerializeField] NetworkObjectsType1SO networkObjectType1SO;

    private float cooldownTime = 1.5f; // Cooldown duration in seconds
    private float lastInteractionTime = -Mathf.Infinity; // Initialize to a very low value


    public void TryInteract(NetworkObject netObjToBeParent = null)
    {
        
        if (Time.time - lastInteractionTime >= cooldownTime)
        {
            Debug.Log("Interactable_SpawnIHoldableItem Called:");
            lastInteractionTime = Time.time; // Update the last interaction time

            if (netObjToBeParent != null)
            {
                GameItemsManager.Instance.fn_SpawnHoldableItem(networkObjectType1SO, netObjToBeParent);
                Debug.Log($" Holdable Object {networkObjectType1SO.ToString()} spawned with parent {netObjToBeParent.transform.name}");
            }
            else
            {
                Debug.LogWarning("Interact is on cooldown. Please wait.");
            }
        }
        else
        {
            Debug.Log("Interact is on cooldown. Please wait.");
        }
    }
}
