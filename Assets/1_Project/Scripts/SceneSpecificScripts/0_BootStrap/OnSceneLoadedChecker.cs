using UnityEngine;
using UnityEngine.SceneManagement;

public class OnSceneLoadedChecker : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Scene is fully loaded and this GameObject is active.");
        GoToMainMenu();
    }

    //private void OnEnable()
    //{
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    //private void OnDisable()
    //{
    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}

    //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    Debug.Log($"Scene '{scene.name}' loaded with mode: {mode}");
    //    GoToMainMenu();

    //}

    private void GoToMainMenu()
    {

        Debug.Log("Scene Load MainMenu");
        if (NetworkSceneManager.Instance != null)
        {
            NetworkSceneManager.Instance.fn_GoToMainMenu();
        }
        else
        {
            Debug.LogWarning("Couldnt Find NetworkSceneManager, Created a New One");
            NetworkSceneManager.fn_CreateNew();
            NetworkSceneManager.Instance.fn_GoToMainMenu();
        }
    }
}
