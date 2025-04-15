using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bite_Activator : NetworkBehaviour
{
    public bool isGrabbing = false;
    public GameObject player;
    public Transform grabPoint;

    void Update()
    {
        
        
        Grab_Update();
    }
    public void StartGrab(GameObject player)
    {
        isGrabbing = true;
        this.player = player;
        Debug.Log("grab player player");
    }

    public void Grab_Update()
    {
        //have a time check.
        if (isGrabbing && player != null)
        {
            MovePlayerToGrabServerRpc(player.GetComponent<NetworkObject>().OwnerClientId, gameObject.GetComponent<NetworkObject>().OwnerClientId, grabPoint.position);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    [ServerRpc]
    public void MovePlayerToGrabServerRpc(ulong PlayerId, ulong id, Vector3 targetPosition)
    {
        // Partent Bitten Object
        Transform p = NetworkManager.Singleton.ConnectedClients[PlayerId].PlayerObject.gameObject.transform;
        Transform t = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.transform;
        p.parent = t;


        //SpawnManager.SpawnedObjects[PlayerId].gameObject
        //GameObject playerObject =   .gameObject;

        NetworkManager.Singleton.ConnectedClients[PlayerId].PlayerObject.gameObject.GetComponent<AnimalCharacter>().DisableMovementRpc();
        //GetComponent<AnimalCharacter>().MoveTo(targetPosition);

        //playerObject.GetComponent<NetworkTransform>().AuthorityMode = AuthorityModes.Server;
        //Debug.Log("moving" + playerObject.name);
        //playerObject
        //playerObject.transform.position = targetPosition;
        //playerObject.GetComponent<NetworkTransform>().AuthorityMode = AuthorityModes.Owner;
    }

    public void CallClientRpcForOne(ulong clientId)
    {
        if (!IsServer)
        {
            Debug.LogError("This Should only be called by the server!!!");
            return;
        }

        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new List<ulong> { clientId } // target just one client
            }
        };

        ShowMessageClientRpc(clientRpcParams);
    }


    [ClientRpc]
    private void ShowMessageClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("This runs only on the target client.");
        Bite_Receiver.Instance.fn_SetBiteMode();
    }

}



