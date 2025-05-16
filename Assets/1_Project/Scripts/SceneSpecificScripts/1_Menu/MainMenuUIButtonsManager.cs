using UnityEngine;

public class MainMenuUIButtonsManager : MonoBehaviour
{
    public void fn_StartHost()
    {
        NetworkSceneManager.Instance.fn_StartHost();
        //if (SceneFowarding.Instance != null)
        //    SceneFowarding.Instance.fn_StartHost();
        //else
        //{
        //    SceneFowarding.fn_CreateNew();
        //    SceneFowarding.Instance.fn_StartHost();
        //}
    }
    public void fn_StartClient()
    {
        NetworkSceneManager.Instance.fn_StartClient();
        //if (SceneFowarding.Instance != null)
        //    SceneFowarding.Instance.fn_StartClient();
        //else
        //{
        //    SceneFowarding.fn_CreateNew();
        //    SceneFowarding.Instance.fn_StartClient();
        //}
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

    

