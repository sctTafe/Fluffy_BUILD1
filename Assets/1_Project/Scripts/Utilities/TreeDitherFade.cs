using UnityEngine;
using System.Collections.Generic;

public class TreeDitherFade : MonoBehaviour
{
    [SerializeField] private float sphereCastRadius = 0.5f;
    [SerializeField] private LayerMask treeLayer;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float minFadeAmount = 0.5f;
    [SerializeField] private float startOffsetDistance = 0.5f;
    [SerializeField] private float offsetEndDistance = -0.5f;

    private Dictionary<Renderer, float> treeTargets = new Dictionary<Renderer, float>();
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector3 start = cam.transform.position - cam.transform.forward * startOffsetDistance;
        Vector3 end = transform.position + cam.transform.forward * offsetEndDistance;
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);

        // Debug the spherecast
        Debug.DrawRay(start, dir * dist, Color.cyan);
        DebugDrawSphereCast(start, dir, dist, sphereCastRadius, Color.green);


        HashSet<Renderer> hitRenderers = new HashSet<Renderer>();

        RaycastHit[] hits = Physics.SphereCastAll(start, sphereCastRadius, dir, dist, treeLayer);
        foreach (var hit in hits)
        {
            Renderer r = hit.collider.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                hitRenderers.Add(r);
                if (!treeTargets.ContainsKey(r))
                    treeTargets[r] = 1f;
            }
        }

        foreach (var r in hitRenderers)
        {
            float current = treeTargets[r];
            float newVal = Mathf.MoveTowards(current, minFadeAmount, fadeSpeed * Time.deltaTime);
            SetDitherAlpha(r, newVal);
            treeTargets[r] = newVal;
        }

        var keys = new List<Renderer>(treeTargets.Keys);
        foreach (var r in keys)
        {
            if (!hitRenderers.Contains(r))
            {
                float current = treeTargets[r];
                float newVal = Mathf.MoveTowards(current, 1f, fadeSpeed * Time.deltaTime);
                SetDitherAlpha(r, newVal);
                treeTargets[r] = newVal;

                if (Mathf.Approximately(newVal, 1f))
                    treeTargets.Remove(r);
            }
        }
    }

    void SetDitherAlpha(Renderer renderer, float value)
    {
        foreach (var mat in renderer.materials)
        {
            if (mat.HasProperty("_DitherAlpha"))
                mat.SetFloat("_DitherAlpha", value);
        }
    }

    // Helper to draw a representation of a spherecast in the editor
    void DebugDrawSphereCast(Vector3 origin, Vector3 direction, float distance, float radius, Color color)
    {
        Vector3 end = origin + direction.normalized * distance;
        DebugDrawWireSphere(origin, radius, color);
        DebugDrawWireSphere(end, radius, color);
    }

    void DebugDrawWireSphere(Vector3 center, float radius, Color color)
    {
        float angle = 10f;
        for (float i = 0; i < 360f; i += angle)
        {
            Vector3 offset1 = Quaternion.Euler(0, i, 0) * Vector3.forward * radius;
            Vector3 offset2 = Quaternion.Euler(0, i + angle, 0) * Vector3.forward * radius;
            Debug.DrawLine(center + offset1, center + offset2, color);
        }
    }
}
