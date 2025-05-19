using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

public class UnityEventButton : MonoBehaviour
{
    public UnityEvent _onButtonPressed;

    public void InvokeEvent()
    {
        _onButtonPressed?.Invoke();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UnityEventButton))]
public class UnityEventButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UnityEventButton myScript = (UnityEventButton)target;

        if (GUILayout.Button("Invoke Testing Event"))
        {
            myScript.InvokeEvent();
        }
    }
}
#endif
