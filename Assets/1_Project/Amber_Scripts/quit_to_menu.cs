using UnityEngine;

public class quit_to_menu : MonoBehaviour
{
	public void ToMenu()
	{
		NetworkSceneManager.Instance.fn_Disconnect();
	}
}
