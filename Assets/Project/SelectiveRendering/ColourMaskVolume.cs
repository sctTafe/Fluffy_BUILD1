using UnityEditor;
using UnityEngine;

public enum ShapeType
{
    Sphere = 0,
    Box = 1,
    Capsule = 2
}

[ExecuteAlways]
[DisallowMultipleComponent]
public class ColourShapeVolume : MonoBehaviour
{
    public ShapeType shapeType;
    [Range(0f,5f)] public float smoothness = 0.1f;

    public Color gizmoColor = new Color(1, 0, 1, 0.2f);
    public bool drawGizmo = true;

    const float radiusScalar = 0.5f; // half sphere size to make it 1:1 scale

    void OnDrawGizmos()
    {
        if (!drawGizmo) return;

        Gizmos.color = gizmoColor;

        switch (shapeType)
        {
            case ShapeType.Sphere:
                float r = transform.lossyScale.x * radiusScalar;
                Gizmos.DrawWireSphere(transform.position, r);
                break;

            case ShapeType.Box:
                var worldExt = Vector3.Scale(Vector3.one, transform.localScale) * 0.5f;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, worldExt * 2);
                Gizmos.matrix = Matrix4x4.identity;
                break;

            case ShapeType.Capsule:
                   // DrawCapsule();
                break;
        }
    }

    //void DrawCapsule()
    //{
    //    float radius = transform.localScale.x * radiusScalar;
    //    float oheight = transform.localScale.y;
    //    float height = Mathf.Max(0, oheight - 2 * radius); // exclude hemispheres
    //    Vector3 dir = Vector3.up;
    //
    //    // Calculate top and bottom centers
    //    Vector3 up = transform.rotation * dir;
    //    Vector3 center = transform.position;
    //    Vector3 top = center + up * (height * 0.5f);
    //    Vector3 bottom = center - up * (height * 0.5f);
    //
    //    // Draw lines connecting hemispheres
    //    Vector3 right = Vector3.Cross(up, Vector3.forward).normalized;
    //    if (right == Vector3.zero)
    //        right = Vector3.Cross(up, Vector3.right).normalized;
    //    Vector3 forward = Vector3.Cross(right, up).normalized;
    //
    //    // Connect side lines
    //    Gizmos.DrawLine(top + right * radius, bottom + right * radius);
    //    Gizmos.DrawLine(top - right * radius, bottom - right * radius);
    //    Gizmos.DrawLine(top + forward * radius, bottom + forward * radius);
    //    Gizmos.DrawLine(top - forward * radius, bottom - forward * radius);
    //
    //    // Draw hemisphere arcs
    //    Handles.color = gizmoColor;
    //    Handles.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
    //
    //    Handles.DrawWireArc(Vector3.down * (height * 0.5f), Vector3.right, Vector3.forward, 180, radius);
    //    Handles.DrawWireArc(Vector3.up * (height * 0.5f), Vector3.right, Vector3.back, 180, radius);
    //    Handles.DrawWireArc(Vector3.down * (height * 0.5f), Vector3.forward, Vector3.left, 180, radius);
    //    Handles.DrawWireArc(Vector3.up * (height * 0.5f), Vector3.forward, Vector3.right, 180, radius);
    //
    //    // Extra rings along the body
    //    {
    //        Handles.DrawWireDisc(Vector3.up * (height * 0.5f), Vector3.up, radius);
    //        Handles.DrawWireDisc(Vector3.zero * (height * 0.5f), Vector3.up, radius);
    //        Handles.DrawWireDisc(Vector3.down * (height * 0.5f), Vector3.up, radius);
    //    }
    //}
}
