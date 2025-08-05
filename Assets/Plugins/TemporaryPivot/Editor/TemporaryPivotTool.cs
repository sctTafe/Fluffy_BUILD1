using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Stateful Temporary Pivot Tool")]
public class TemporaryPivotTool : EditorTool
{
    private enum PivotEditState
    {
        Placing,
        Editing
    }

    private static bool toolEnabled = false;
    private static PivotEditState editState = PivotEditState.Placing;

    private static Vector3 tempPivot;
    private static Quaternion tempRotation;
    private static bool hasPivot = false;

    private static bool useLocalRotation = true;

    public override void OnActivated()
    {
        if (Selection.activeTransform != null)
        {
            tempPivot = Selection.activeTransform.position;
            tempRotation = Selection.activeTransform.rotation;
            hasPivot = true;
        }
    }

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView)) return;
        if (Selection.activeTransform == null) return;

        Transform target = Selection.activeTransform;
        Event e = Event.current;

        // === GUI ===
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 250, 150), "Temporary Pivot Tool", "Window");

        if (!toolEnabled)
        {
            if (GUILayout.Button("Enable Tool"))
            {
                toolEnabled = true;
                editState = PivotEditState.Placing;
                tempPivot = target.position;
                tempRotation = target.rotation;
                hasPivot = true;
            }
        }
        else
        {
            if (GUILayout.Button("Disable Tool"))
            {
                toolEnabled = false;
            }

            useLocalRotation = GUILayout.Toggle(useLocalRotation, "Use Local Rotation");

            if (editState == PivotEditState.Placing)
            {
                GUILayout.Label("Click in scene to set pivot");
                if (GUILayout.Button("Confirm Pivot"))
                    editState = PivotEditState.Editing;
            }
            else if (editState == PivotEditState.Editing)
            {
                GUILayout.Label("Transform object using pivot");
                if (GUILayout.Button("Return to Pivot Placement"))
                    editState = PivotEditState.Placing;
            }

            if (GUILayout.Button("Clear Pivot"))
                hasPivot = false;
        }

        GUILayout.EndArea();
        Handles.EndGUI();

        if (!toolEnabled || !hasPivot) return;

        // === DRAW AND HANDLE PIVOT PLACEMENT ===
        if (editState == PivotEditState.Placing)
        {
            Handles.color = Color.cyan;

            EditorGUI.BeginChangeCheck();
            Vector3 newPivot = Handles.PositionHandle(tempPivot, useLocalRotation ? tempRotation : Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                tempPivot = newPivot;
            }

            Handles.SphereHandleCap(0, tempPivot, Quaternion.identity, HandleUtility.GetHandleSize(tempPivot) * 0.08f, EventType.Repaint);
            Handles.Label(tempPivot + Vector3.up * 0.2f, "Placing Pivot");
            Handles.DrawDottedLine(tempPivot, target.position, 4f);

            Event ev = Event.current;

            // Only handle mouse clicks if NOT dragging the gizmo
            if (ev.type == EventType.MouseDown && ev.button == 0)
            {
                // Check if the mouse is over the position handle
                int id = GUIUtility.hotControl;
                int nearest = HandleUtility.nearestControl;

                // If no control is active (not dragging handle) and mouse is not over handle, place pivot
                if (id == 0 && nearest == 0)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
                    if (Physics.Raycast(ray, out var hit))
                        tempPivot = hit.point;
                    else
                        tempPivot = ray.origin + ray.direction * 10f;

                    tempRotation = target.rotation;
                    ev.Use();
                }
                // Else ignore to allow gizmo drag
            }
        }

        // === DRAW PIVOT HANDLE ===
        Handles.color = (editState == PivotEditState.Placing) ? Color.cyan : Color.yellow;
        Handles.SphereHandleCap(0, tempPivot, Quaternion.identity, HandleUtility.GetHandleSize(tempPivot) * 0.08f, EventType.Repaint);
        Handles.Label(tempPivot + Vector3.up * 0.2f, editState == PivotEditState.Placing ? "Placing Pivot" : "Active Pivot");
        Handles.DrawDottedLine(tempPivot, target.position, 4f);

        // === HANDLE TRANSFORMING OBJECT FROM PIVOT ===
        if (editState == PivotEditState.Editing)
        {
            Quaternion handleRot = useLocalRotation ? tempRotation : Quaternion.identity;

            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(tempPivot, handleRot);
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 delta = newPos - tempPivot;
                target.position += delta;
                tempPivot = newPos;
            }

            EditorGUI.BeginChangeCheck();
            Quaternion newRot = Handles.RotationHandle(tempRotation, tempPivot);
            if (EditorGUI.EndChangeCheck())
            {
                Quaternion deltaRot = newRot * Quaternion.Inverse(tempRotation);
                target.RotateAround(tempPivot, deltaRot * Vector3.forward, deltaRot.eulerAngles.z);
                target.RotateAround(tempPivot, deltaRot * Vector3.up, deltaRot.eulerAngles.y);
                target.RotateAround(tempPivot, deltaRot * Vector3.right, deltaRot.eulerAngles.x);
                tempRotation = newRot;
            }
        }
    }

    [MenuItem("Tools/Temporary Pivot Tool/Activate Tool")]
    private static void ActivateTool()
    {
        ToolManager.SetActiveTool<TemporaryPivotTool>();
    }
}
