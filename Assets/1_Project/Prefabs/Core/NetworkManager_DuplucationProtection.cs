using Unity.Netcode;
using UnityEngine;

public class NetworkManager_DuplucationProtection : MonoBehaviour
{
    void Awake()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton != this)
        {
            Destroy(gameObject); 
            return;
        }

        DontDestroyOnLoad(gameObject); 
    }
}
