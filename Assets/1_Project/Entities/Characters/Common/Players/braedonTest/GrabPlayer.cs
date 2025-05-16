
using Unity.Netcode;

using UnityEngine;


public class GrabPlayer : NetworkBehaviour
{
    public bool isGrabbing = false;

    public GameObject player;

    public Transform grabPoint;
    //public 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Grab();
    }

    public void StartGrab(GameObject player)
    {
        isGrabbing = true;
        this.player = player;
        Debug.Log("grab player player");
    }

    public void Grab()
    {
        //have a time check.
        if (isGrabbing && player != null)
        {
            MovePlayerToGrabServerRpc(player.GetComponent<NetworkObject>().OwnerClientId, gameObject.GetComponent<NetworkObject>().OwnerClientId, grabPoint.position);
        }
    }


    public bool IsGrabbing()
    {
        return isGrabbing;
    }

    [ServerRpc]
    public void MovePlayerToGrabServerRpc(ulong PlayerId, ulong id, Vector3 targetPosition)
    {
        //SpawnManager.SpawnedObjects[PlayerId].gameObject
        //GameObject playerObject =   .gameObject;
        Transform p = NetworkManager.Singleton.ConnectedClients[PlayerId].PlayerObject.gameObject.transform;
        Transform t = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.transform;
        p.parent = t;


        NetworkManager.Singleton.ConnectedClients[PlayerId].PlayerObject.gameObject.GetComponent<AnimalCharacter>().DisableMovementRpc();
        //GetComponent<AnimalCharacter>().MoveTo(targetPosition);

        //playerObject.GetComponent<NetworkTransform>().AuthorityMode = AuthorityModes.Server;
        //Debug.Log("moving" + playerObject.name);
        //playerObject
        //playerObject.transform.position = targetPosition;
        //playerObject.GetComponent<NetworkTransform>().AuthorityMode = AuthorityModes.Owner;
    }



}
