using UnityEngine;
using UnityEngine.SceneManagement;

public class pause_menu : MonoBehaviour
{

	bool paused = false;
	public GameObject pause_ui;

	void Start()
	{
		pause_ui.SetActive(false);
	}

    void Update()
    {
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			paused = !paused;		
			pause_ui.SetActive(paused);
			// cursorControler.SetCursorState(!paused);

			if(paused)
			{
				Cursor.lockState = CursorLockMode.None;
			}
			else
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
    }

	public void ToMenu()
	{
		SceneManager.LoadScene(1);
	}
}
