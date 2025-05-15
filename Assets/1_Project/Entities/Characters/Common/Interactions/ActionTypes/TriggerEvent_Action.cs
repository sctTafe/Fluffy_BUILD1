using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class ActivateAction : InteractionAction
{
    [Header("Activate Action Settings")]
    // UnityEvent allows you to attach any methods from scripts via the Inspector.
    [SerializeField] private UnityEvent onActivate;
    [SerializeField] private string prompt = "Activate";

    public override void Execute(PlayerInteractor player)
    {
        if (IsServer)
        {
            TriggerActivate();
        }
        else
        {
            // Request the server to trigger activation.
            ExecuteActivateServerRpc();
        }
    }

    // This method triggers the UnityEvent on the server.
    private void TriggerActivate()
    {
        Debug.Log($"[Server] {gameObject.name} activated.");
        onActivate?.Invoke();
    }

    // ServerRpc to ensure activation is handled on the server.
    [ServerRpc(RequireOwnership = false)]
    private void ExecuteActivateServerRpc(ServerRpcParams rpcParams = default)
    {
        TriggerActivate();
    }
    public override string GetActionName()
    {
        return prompt;
    }
}
