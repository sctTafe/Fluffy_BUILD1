using UnityEngine;

public class MainMenuUIButtonsManager : MonoBehaviour
{
    public void fn_StartHost()
    {
        NetworkSceneManager.Instance.fn_StartHost();
    }
    public void fn_StartClient()
    {
        NetworkSceneManager.Instance.fn_StartClient();
    }

    public void fn_NextScene()
    {
        NetworkSceneManager.Instance.fn_SceneSwitch_NextScene();
    }

    public void fn_GoToLandingPage()
    {
        NetworkSceneManager.Instance.fn_GoToMainMenu();
    }

    public void fn_Cerdits()
    {
        NetworkSceneManager.Instance.fn_GoToScene("7_Credits");
    }
    public void fn_QuitGame()
    {
        NetworkSceneManager.Instance.fn_QuitGame();
    }
}

    

