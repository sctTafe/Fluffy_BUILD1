using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


public class Scott_CanvasDisabler : MonoBehaviour
{
    [Header("Canvas Management")]
    [Tooltip("List of all canvases to control. Assign these in the inspector.")]
    public List<Canvas> canvasesToControl = new List<Canvas>();

    [Header("Input Settings")]
    [Tooltip("The key code to press for toggling canvases")]
    public KeyCode toggleKey = KeyCode.Alpha0;

    [Header("Toggle Behavior")]
    [Tooltip("If true, toggles between on/off. If false, always turns off when pressed.")]
    public bool toggleMode = false;

    // Track the current state of canvases
    private bool canvasesAreActive = true;

    void Start()
    {
        // Initialize canvas state tracking
        UpdateCanvasState();
    }

    void Update()
    {
        // Check for key input
        if (Input.GetKeyDown(toggleKey))
        {
                ToggleCanvases();
        }
    }

    /// <summary>
    /// Toggles all canvases between active and inactive
    /// </summary>
    public void ToggleCanvases()
    {
        canvasesAreActive = !canvasesAreActive;
        if (canvasesAreActive)
        {
            TurnOnAllCanvases();
        }
        else
        {
            TurnOnAllCanvases();
        }

        Debug.Log($"Canvases toggled: {(canvasesAreActive ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Turns off all canvases
    /// </summary>
    public void TurnOffAllCanvases()
    {
        canvasesAreActive = false;
        SetCanvasesActive(false);

        Debug.Log("All canvases turned OFF");
    }

    /// <summary>
    /// Turns on all canvases
    /// </summary>
    public void TurnOnAllCanvases()
    {
        canvasesAreActive = true;
        SetCanvasesActive(true);

        Debug.Log("All canvases turned ON");
    }

    /// <summary>
    /// Auto-populate the canvas list with all canvases in the scene (Editor only) - Only finds active ones
    /// </summary>
    [ContextMenu("Find All Canvases in Scene")]
    public void FindAllCanvasesInScene()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        canvasesToControl.Clear();
        canvasesToControl.AddRange(allCanvases);

        Debug.Log($"Found and added {allCanvases.Length} canvases to the control list.");
    }

    // Public methods that can be called from UI buttons or other scripts
    public void SetToggleMode(bool toggle) => toggleMode = toggle;
    public void SetToggleKey(KeyCode key) => toggleKey = key;



    /// <summary>
    /// Sets the active state of all canvases in the list
    /// </summary>
    /// <param name="active">Whether canvases should be active or not</param>
    private void SetCanvasesActive(bool active)
    {
        foreach (Canvas canvas in canvasesToControl)
        {
            if (canvas != null)
            {
                canvas.gameObject.SetActive(active);
            }
            else
            {
                Debug.LogWarning("CanvasToggleController: Found null canvas in the list!");
            }
        }
    }

    /// <summary>
    /// Updates the internal state based on current canvas visibility
    /// </summary>
    private void UpdateCanvasState()
    {
        if (canvasesToControl.Count > 0 && canvasesToControl[0] != null)
        {
            canvasesAreActive = canvasesToControl[0].gameObject.activeInHierarchy;
        }
    }


}
#if UNITY_EDITOR
[CustomEditor(typeof(Scott_CanvasDisabler))]
public class CanvasToggleControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Scott_CanvasDisabler controller = (Scott_CanvasDisabler)target;

        // Draw the default inspector
        DrawDefaultInspector();

        // Add some space
        EditorGUILayout.Space();

        // Add a section header
        EditorGUILayout.LabelField("Auto-Population", EditorStyles.boldLabel);

        // Add the auto-populate button
        if (GUILayout.Button("Find All Canvases in Scene", GUILayout.Height(30)))
        {
            // Record the object for undo
            Undo.RecordObject(controller, "Auto-populate Canvas List");

            // Find and populate canvases
            controller.FindAllCanvasesInScene();

            // Mark the object as dirty so changes are saved
            EditorUtility.SetDirty(controller);
        }

        // Add help text
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Click 'Find All Canvases in Scene' to automatically populate the canvas list with all Canvas components found in the current scene.", MessageType.Info);

        // Show current canvas count
        EditorGUILayout.LabelField($"Current Canvas Count: {controller.canvasesToControl.Count}");
    }
}
#endif