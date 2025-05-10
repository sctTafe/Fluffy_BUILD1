using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class DestructibleObject_Reciver : NetworkBehaviour
{
    private const bool ISDEBUGGING = true;

    public UnityEvent _onDestructibleGettingAttacked;

    internal void fn_TriggerDestruction(float delay = 0f)
    {
        if (ISDEBUGGING) Debug.Log("DestructibleObject_Reciver: fn_TriggerDestruction Called!");
        SendOnAttackedRpc(delay);
        StartCoroutine(DelayedDestoryFunctionCall(delay));

    }

    void Start()
    {
        
    }

    IEnumerator DelayedDestoryFunctionCall(float delay)
    {
        // Wait for 1.5 seconds
        yield return new WaitForSeconds(delay);

        // Call the function after the delay
        if (ISDEBUGGING) Debug.Log("DestructibleObject_Reciver: DestroyObjectServerRPC Called!");
        DestroyObjectServerRPC(this.NetworkObjectId);
    }

    #region Network RPCs
    // --- _onDestructibleGettingAttacked ---
    // ServerRpc - called from client, runs on server
    [Rpc(SendTo.Server)]
    private void SendOnAttackedRpc(float delay = 0f)
    {
        SendOnAttackedClientRpc(delay);
    }

    // ClientRpc - called from server, runs on all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void SendOnAttackedClientRpc(float delay = 0f)
    {
        _onDestructibleGettingAttacked?.Invoke();

        if (ISDEBUGGING) Debug.Log("DestructibleObject_Reciver: ClientRPC _onDestructibleGettingAttacked Called!");
    }


    // --- Destory This Object ---

    [ServerRpc(RequireOwnership = false)]
    private void DestroyObjectServerRPC(ulong to_destroy)
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(to_destroy, out NetworkObject target_object);
            target_object.Despawn();
        }
    }

    #endregion







}
