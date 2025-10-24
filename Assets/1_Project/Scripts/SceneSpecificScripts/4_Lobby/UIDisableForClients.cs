using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UIDisableForClients : NetworkBehaviour
{
    [Tooltip("Assign any GameObjects you want to disable for clients (non-hosts).")]
    [SerializeField] private List<GameObject> objectsToDisable = new List<GameObject>();

    public override void OnNetworkSpawn()
    {
        // Only run this check when the object is spawned over the network
        if (!IsHost)
        {
            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }
}