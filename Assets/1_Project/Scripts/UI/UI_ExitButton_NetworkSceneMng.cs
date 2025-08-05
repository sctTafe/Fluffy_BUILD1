using UnityEngine;
using UnityEngine.UI;

public class UI_ExitButton_NetworkSceneMng : MonoBehaviour
{
    public Button _exitButton;

    NetworkSceneManager _currentNSM;
    private void Start()
    {
        if(_exitButton == null)
            _exitButton = GetComponent<Button>();

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
