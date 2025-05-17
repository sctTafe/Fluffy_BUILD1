using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Singleton For NetworkBehaviors
/// </summary>
public class NetworkSingleton<T> : NetworkBehaviour
    where T : Component
{

    private static T _instance;

    //private void Awake()
    //{
    //    if (NetworkSingleton<T>.Instance != null && NetworkSingleton<T>.Instance != this)
    //    {
    //        Debug.LogWarning($"Wanrning the is a duplicate of {NetworkSingleton<T>.Instance.name}");
    //        Destroy(NetworkSingleton<T>.Instance);
    //        return;
    //    }
    //}
    
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // Use FindObjectsByType with FindObjectsSortMode.None for better performance.
                var objs = FindObjectsByType<T>(FindObjectsSortMode.None);
                //var objs = FindObjectsOfType(typeof(T)) as T[];

                if (objs.Length > 0)
                    _instance = objs[0];
                if (objs.Length > 1)
                {
                    Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                }
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = string.Format("_{0}", typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
}
