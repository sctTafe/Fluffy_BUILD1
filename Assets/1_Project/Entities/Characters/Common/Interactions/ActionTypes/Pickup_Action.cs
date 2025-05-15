using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(InteractableObject))]
public class PickupAction : InteractionAction
{
    public GameObject itemToPickup;
    private Rigidbody rb;
    private NetworkObject networkObject;
    [SerializeField] private string prompt = "Pickup";

    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    public override void Execute(PlayerInteractor player)
    {
        if (IsServer)
        {
            AttachToPlayer(player.GetComponent<PlayerController>());
        }
        else
        {
            ExecutePickupServerRpc(player.GetComponent<NetworkObject>());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ExecutePickupServerRpc(NetworkObjectReference playerRef, ServerRpcParams rpcParams = default)
    {
        if (playerRef.TryGet(out NetworkObject playerObject))
        {
            AttachToPlayer(playerObject.GetComponent<PlayerController>());
        }
    }

    private void AttachToPlayer(PlayerController player)
    {
        if (player == null || itemToPickup == null)
            return;

        // Transfer ownership to the player so they can control it
        //networkObject.ChangeOwnership(player.OwnerClientId);

        // Example of attaching item to the player
        var playerJoint = player.GetComponentInChildren<FixedJoint>();
        if (playerJoint != null)
        {
            rb = itemToPickup.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log($"{gameObject.name} is being picked up by {player.name}");
                rb.isKinematic = true; // Disable physics while held
                itemToPickup.transform.SetParent(player.transform, true);
            }
        }
    }

    public override string GetActionName()
    {
        return prompt;
    }
}
