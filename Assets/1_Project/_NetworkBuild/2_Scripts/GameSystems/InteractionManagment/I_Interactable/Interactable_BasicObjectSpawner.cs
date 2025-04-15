using Unity.Netcode;
using UnityEngine;

public class Interactable_BasicObjectSpawner : MonoBehaviour, IInteractable
{
    [SerializeField] NetworkObjectsType1SO networkObjectType1SO;

    private float cooldownTime = 1.5f; // Cooldown duration in seconds
    private float lastInteractionTime = -Mathf.Infinity; // Initialize to a very low value


    public void TryInteract(NetworkObject interactionOwner = null)
    {
        if (Time.time - lastInteractionTime >= cooldownTime)
        {
            // Update the last interaction time
            lastInteractionTime = Time.time;

            GameItemsManager.Instance.fn_SpawnNetworkObjectType1(networkObjectType1SO, this.transform.position + Vector3.up * 0.5f);
        }
        else
        {
            Debug.Log("Interact is on cooldown. Please wait.");
        }
    }

}
