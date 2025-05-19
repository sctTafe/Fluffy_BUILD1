using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;


public class SceneLoadWatcher : NetworkBehaviour
{
    //private void OnEnable()
    //{
    //    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnNetworkSceneLoadComplete;
    //}

    //private void OnNetworkSceneLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    //{
        
    //}

    //private void OnDisable()
    //{
    //    if (NetworkManager.Singleton != null)
    //    {
    //        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnNetworkSceneLoadComplete;
    //    }
    //}

    //private void OnNetworkSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    //{
    //    Debug.Log($"Client {clientId} finished loading scene: {sceneName}");

    //    // Optional: Check if ALL clients are done loading
    //    if (NetworkManager.Singleton.IsServer)
    //    {
    //        bool allClientsLoaded = true;
    //        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
    //        {
    //            if (!NetworkManager.Singleton.SceneManager.IsSceneLoadedByClient(client.ClientId, sceneName))
    //            {
    //                allClientsLoaded = false;
    //                break;
    //            }
    //        }

    //        if (allClientsLoaded)
    //        {
    //            Debug.Log("All clients finished loading the scene.");
    //            // You can now start gameplay, spawn objects, etc.
    //        }
    //    }
    //}
}

