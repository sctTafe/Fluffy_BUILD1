using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class InteractableObject : NetworkBehaviour
{
    private Dictionary<ActionKey, InteractionAction> interactionMap;

    private void Awake()
    {
        interactionMap = new Dictionary<ActionKey, InteractionAction>();
        foreach (var action in GetComponents<InteractionAction>())
        {
            if (!interactionMap.ContainsKey(action.actionKey))
                interactionMap.Add(action.actionKey, action);
        }
    }

    public List<(ActionKey actionKey, string actionName)> GetAssignedActions()
    {
        return interactionMap.Select(entry => (entry.Key, entry.Value.GetActionName())).ToList();
    }

    public void Interact(ActionKey actionKey, PlayerInteractor player)
    {
        if (interactionMap.TryGetValue(actionKey, out InteractionAction action))
        {
            action.Execute(player);
        }
        else
        {
            Debug.LogWarning($"No action assigned to {actionKey} on {gameObject.name}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            var netObj = other.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                Debug.Log($"[Server] {gameObject.name} triggered by client {netObj.OwnerClientId}");
                RegisterInteractableClientRpc(netObj.OwnerClientId);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            var netObj = other.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                Debug.Log($"[Server] {gameObject.name} exit triggered by client {netObj.OwnerClientId}");
                UnregisterInteractableClientRpc(netObj.OwnerClientId);
            }
        }
    }

    [ClientRpc]
    private void RegisterInteractableClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        Debug.Log($"[Client] Registering {gameObject.name} on client {clientId}");
        if (PlayerInteractor.LocalInstance != null)
            PlayerInteractor.LocalInstance.RegisterInteractable(this);
        else
            Debug.LogWarning("[Client] PlayerInteractor.LocalInstance is null");
    }

    [ClientRpc]
    private void UnregisterInteractableClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        Debug.Log($"[Client] Unregistering {gameObject.name} on client {clientId}");
        if (PlayerInteractor.LocalInstance != null)
            PlayerInteractor.LocalInstance.UnregisterInteractable(this);
        else
            Debug.LogWarning("[Client] PlayerInteractor.LocalInstance is null");
    }
}
