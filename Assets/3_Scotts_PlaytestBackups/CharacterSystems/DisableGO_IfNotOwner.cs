using Unity.Netcode;
using UnityEngine;
public class DisableGO_IfNotOwner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
    }
}
