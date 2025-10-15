using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Logs the scene in player prefs
/// </summary>
public class SceneTrackerMng : MonoBehaviour
{
    
    [SerializeField] bool _UpdatePlayerPref_LastScene = false;

    private const string SCENE_KEY = "LastSceneName";

    private void Start()
    {
        if(_UpdatePlayerPref_LastScene)
            Update_LastScene();
    }

    /// <summary>
    /// Checks If the saved scene is the same as 'lastSceneString'
    /// </summary>
    public bool IsLastScene(string lastSceneString)
    {
        if (lastSceneString == GetLastSavedScene())
            return true;

        return false;
    }


    /// <summary>
    /// Saves the current scene name to PlayerPrefs.
    /// </summary>
    private void Update_LastScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(SCENE_KEY, sceneName);
        PlayerPrefs.Save();
        Debug.Log($"Saved scene '{sceneName}' to PlayerPrefs.");
    }

    /// <summary>
    /// Retrieves the last saved scene name from PlayerPrefs.
    /// Returns null if none is found.
    /// </summary>
    public string GetLastSavedScene()
    {
        if (PlayerPrefs.HasKey(SCENE_KEY))
        {
            return PlayerPrefs.GetString(SCENE_KEY);
        }
        else
        {
            Debug.LogWarning("No scene name found in PlayerPrefs.");
            return null;
        }
    }
}
