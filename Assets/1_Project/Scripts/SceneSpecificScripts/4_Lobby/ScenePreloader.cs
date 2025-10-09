using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class ScenePreloader : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "N_IslandReplacement_Preload";

    LobbyManager preGameLobbyManager;
    private void Start()
    {
        preGameLobbyManager = LobbyManager.Instance;
        StartPreload();
    }

    public void StartPreload()
    {
        StartCoroutine(PreloadScene(targetSceneName));
    }

    private IEnumerator PreloadScene(string sceneName)
    {
        Debug.Log($"[ScenePreloader] Preloading scene '{sceneName}'...");

        // Preload additively but don't activate
        var preloadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        preloadOp.allowSceneActivation = false;

        while (preloadOp.progress < 0.9f)
        {
            Debug.Log(preloadOp.progress);
            yield return null;
        }



        Debug.Log("[ScenePreloader] Activating preloaded scene (will not be visible)...");
        preloadOp.allowSceneActivation = true;

        // Wait one frame for the activation to finish
        yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (!loadedScene.isLoaded)
        {
            Debug.LogWarning("[ScenePreloader] Scene did not load properly before unload attempt.");
            yield return null;
        }

        foreach (var go in loadedScene.GetRootGameObjects())
            go.SetActive(false);

        // Unload without ever activating
        Debug.Log("[ScenePreloader] Scene preloaded, Unloading");
        //preloadOp.allowSceneActivation = true;
        yield return SceneManager.UnloadSceneAsync(sceneName);

        preGameLobbyManager.fn_PlayerLoaded();
        yield return null;
    }

    private void SceneUnloadedDebug()
    {
        Debug.Log("The scene is actually Unloaded");
    }
}
