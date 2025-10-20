using UnityEngine;
using System.Collections.Generic;

public class TreeDitherFade : MonoBehaviour
{
    [SerializeField] private float startSphereCastRadius = 0.5f;
    [SerializeField] private float endSphereCastRadius = 0.5f;
    [SerializeField, Range(2, 12)] private int castSamples = 3; // number of samples along the path to approximate a tapered cast
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
        DebugDrawSphereCast(start, dir, dist, startSphereCastRadius, endSphereCastRadius, Color.green);


        HashSet<Renderer> hitRenderers = new HashSet<Renderer>();

        // Approximate a tapered cast by sampling overlap spheres along the path
        int samples = Mathf.Max(2, castSamples);
        for (int i = 0; i < samples; i++)
        {
            float t = samples == 1 ? 0f : (float)i / (samples - 1);
            Vector3 p = Vector3.Lerp(start, end, t);
            float r = Mathf.Lerp(startSphereCastRadius, endSphereCastRadius, t);
            Collider[] overlaps = Physics.OverlapSphere(p, r, treeLayer, QueryTriggerInteraction.Ignore);
            for (int j = 0; j < overlaps.Length; j++)
            {
                var col = overlaps[j];
                Renderer rend = col.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    hitRenderers.Add(rend);
                    if (!treeTargets.ContainsKey(rend))
                        treeTargets[rend] = 1f;
                }
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

    // Helper to draw a representation of a tapered spherecast in the editor
    void DebugDrawSphereCast(Vector3 origin, Vector3 direction, float distance, float startRadius, float endRadius, Color color)
    {
        Vector3 end = origin + direction.normalized * distance;
        DebugDrawWireSphere(origin, startRadius, color);
        DebugDrawWireSphere(end, endRadius, color);
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
