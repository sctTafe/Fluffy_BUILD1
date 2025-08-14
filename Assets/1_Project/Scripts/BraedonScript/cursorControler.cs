using UnityEngine;

public class cursorControler : MonoBehaviour
{

    public bool cursorLocked = true;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetCursorState(cursorLocked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(hasFocus);
    }

    public static void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
