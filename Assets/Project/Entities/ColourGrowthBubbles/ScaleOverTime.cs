using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GrowthScalar : MonoBehaviour
{
    public float duration = 3f;
    public AnimationCurve growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public UnityEvent onGrowthComplete;

    private Material objMaterial;
    private Transform objTransform;

    public bool autoStart = false;
    void Start()
    {
        objTransform = transform;

        // Create a new material instance for this object to avoid affecting others
        if (GetComponent<Renderer>())
        {
            objMaterial = new Material(GetComponent<Renderer>().material);
            GetComponent<Renderer>().material = objMaterial;

            // Start fully transparent
            if (objMaterial != null)
                objMaterial.SetFloat("_Fade", 0);
        }
        if (autoStart) 
            StartCoroutine(AnimateGrowthAndFade());
    }

    public void GrowObject()
    {
        StartCoroutine(AnimateGrowthAndFade());
    }


    IEnumerator AnimateGrowthAndFade()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Apply easing from curves
            float scaleFactor = growthCurve.Evaluate(t);
            float alphaFactor = fadeCurve.Evaluate(t);

            objTransform.localScale = Vector3.one * scaleFactor;
            if (objMaterial != null)
                objMaterial.SetFloat("_Fade", alphaFactor);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final values are set
        objTransform.localScale = Vector3.one * growthCurve.Evaluate(1);
        if (objMaterial != null)
            objMaterial.SetFloat("_Fade", fadeCurve.Evaluate(1));

        onGrowthComplete?.Invoke();
        // Disable object after fading out
        //if (fadeCurve.Evaluate(1) == 0)
        {
            //gameObject.SetActive(false);
        }
    }
}
