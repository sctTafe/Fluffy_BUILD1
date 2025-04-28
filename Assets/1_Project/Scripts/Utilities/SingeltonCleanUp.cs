using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Removes the games singletons on return to Main Menu
/// </summary>
public class SingeltonCleanUp : MonoBehaviour
{
    //Singelton Prefabs
    [SerializeField] Transform _NetworkManagerPrefab;
    

    private void Start()
    {
        //Debug.LogWarning("SingeltonCleanUp Called!");

        //if (NetworkManager.Singleton != null)           
        //{
        //    NetworkManager.Singleton.Shutdown();
        //    Destroy(NetworkManager.Singleton.gameObject);
        //    Instantiate(_NetworkManagerPrefab);
        //}

        if (NetworkManager.Singleton == null)
        {
            Instantiate(_NetworkManagerPrefab);
        }
    }
}
