using Unity.Netcode;
using UnityEngine;

public class Interactable_BasicObjectSpawnerWithParent : MonoBehaviour, IInteractable
{
    [SerializeField] NetworkObjectsType1SO networkObjectType1SO;

    private float cooldownTime = 1.5f; // Cooldown duration in seconds
    private float lastInteractionTime = -Mathf.Infinity; // Initialize to a very low value

    public void TryInteract(NetworkObject netObjToBeParent = null)
    {
        if (Time.time - lastInteractionTime >= cooldownTime)
        {
            Debug.Log("Interactable_BasicObjectSpawnerWithParent Called:");
            lastInteractionTime = Time.time; // Update the last interaction time
            if (netObjToBeParent == null)
            {
                //MainGameManager.Instance.fn_SpawnNetworkObjectType1(networkObjectType1SO);
                GameItemsManager.Instance.fn_SpawnNetworkObjectType1(networkObjectType1SO, this.transform.position + Vector3.up * 0.5f);
                Debug.Log($"Object {networkObjectType1SO.ToString()} spawned without parent");
            }
            else
            {
                //MainGameManager.Instance.fn_SpawnNetworkObjectType1(networkObjectType1SO, netObjToBeParent);
                GameItemsManager.Instance.fn_SpawnNetworkObjectType1(networkObjectType1SO, netObjToBeParent);
                Debug.Log($"Object {networkObjectType1SO.ToString()} spawned with parent");
            }
        }
        else
        {
            Debug.Log("Interact is on cooldown. Please wait.");
        }
    }
}
