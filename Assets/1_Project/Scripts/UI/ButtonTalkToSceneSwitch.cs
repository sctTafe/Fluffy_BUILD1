using UnityEngine;
using UnityEngine.UI;

public class ButtonTalkToSceneSwitch : MonoBehaviour
{
    public Button _exitButton;

    NetworkSceneManager _currentNSM;
    private void Start()
    {
        _currentNSM = NetworkSceneManager.Instance;
        if (_currentNSM != null)
        {
            if (_exitButton != null)
                _exitButton?.onClick.AddListener(() =>
                {
                    fn_GoToMainMenu();
                });
        }
    }

    public void fn_GoToMainMenu()
    {
        if (NetworkSceneManager.Instance != null)
        {
            NetworkSceneManager.Instance.fn_Disconnect();
        }
        else 
        {
            Debug.LogWarning("Cant Find Network Scene Manger! Creating new to change scene!");
            GameObject go = new GameObject("TempNetworkManger");
            go.AddComponent<NetworkSceneManager>();
            NetworkSceneManager.Instance.fn_Disconnect();
        }

    }
}
