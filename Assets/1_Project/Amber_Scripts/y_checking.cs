using UnityEngine;
using Unity.Netcode;

public class y_checking : NetworkBehaviour
{
    void Update()
    {
		if(IsOwner && transform.position.y < -7)
		{
			transform.position = new Vector3(0,1,0);
		}
        
    }
}
