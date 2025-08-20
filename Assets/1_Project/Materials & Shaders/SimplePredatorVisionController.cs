using UnityEngine;
using System.Collections;

public class SimplePredatorVisionController : MonoBehaviour
{
    [Header("Reveal Settings")]
    [SerializeField] private float revealDuration = 8f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("Shader Settings")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Color highlightColor = new Color(1f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Audio")]
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip deactivationSound;
    [SerializeField] private AudioSource audioSource;
    
    // State tracking
    private bool isRevealActive = false;
    private Coroutine currentRevealCoroutine;
    private float currentIntensity = 0f;
    
    // Shader property IDs for performance
    private static readonly int HighlightColorID = Shader.PropertyToID("_HighlightColor");
    private static readonly int PulseSpeedID = Shader.PropertyToID("_PulseSpeed");
    private static readonly int EffectIntensityID = Shader.PropertyToID("_EffectIntensity");
    
    // Events
    public System.Action<bool> OnRevealStateChanged;
    public System.Action<float> OnIntensityChanged;
    
    private void Start()
    {
        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D sound
            }
        }
        
        // Create highlight material if not assigned
        if (highlightMaterial == null)
        {
            highlightMaterial = CreateHighlightMaterial();
        }
    }

    /// <summary>
    /// Activates the predator vision reveal effect
    /// </summary>
    public void ActivatePredatorVision(float duration = -1f)
    {
        if (duration == -1f)
            duration = revealDuration;
            
        Debug.Log($"SimplePredatorVisionController: Activating predator vision for {duration} seconds");
        
        // Stop any existing reveal
        if (currentRevealCoroutine != null)
        {
            StopCoroutine(currentRevealCoroutine);
        }
        
        currentRevealCoroutine = StartCoroutine(RevealSequence(duration));
    }

    /// <summary>
    /// Deactivates the predator vision reveal effect
    /// </summary>
    public void DeactivatePredatorVision()
    {
        Debug.Log("SimplePredatorVisionController: Deactivating predator vision");
        
        if (currentRevealCoroutine != null)
        {
            StopCoroutine(currentRevealCoroutine);
            currentRevealCoroutine = null;
        }
        
        StartCoroutine(FadeOutEffect());
    }

    /// <summary>
    /// Main reveal sequence coroutine
    /// </summary>
    private IEnumerator RevealSequence(float duration)
    {
        isRevealActive = true;
        OnRevealStateChanged?.Invoke(true);
        
        // Play activation sound
        PlaySound(activationSound);
        
        // Fade in effect
        yield return StartCoroutine(FadeInEffect());
        
        // Wait for duration (if not indefinite)
        if (duration > 0f)
        {
            float remainingTime = duration - fadeInDuration - fadeOutDuration;
            if (remainingTime > 0f)
            {
                yield return new WaitForSeconds(remainingTime);
            }
        }
        
        // Fade out effect
        yield return StartCoroutine(FadeOutEffect());
        
        isRevealActive = false;
        OnRevealStateChanged?.Invoke(false);
        currentRevealCoroutine = null;
    }

    /// <summary>
    /// Fade in effect coroutine
    /// </summary>
    private IEnumerator FadeInEffect()
    {
        float timer = 0f;
        
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / fadeInDuration;
            float intensity = fadeInCurve.Evaluate(normalizedTime);
            
            UpdateEffectIntensity(intensity);
            OnIntensityChanged?.Invoke(intensity);
            
            yield return null;
        }
        
        UpdateEffectIntensity(1f);
        OnIntensityChanged?.Invoke(1f);
    }

    /// <summary>
    /// Fade out effect coroutine
    /// </summary>
    private IEnumerator FadeOutEffect()
    {
        float timer = 0f;
        
        // Play deactivation sound
        PlaySound(deactivationSound);
        
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / fadeOutDuration;
            float intensity = fadeOutCurve.Evaluate(normalizedTime);
            
            UpdateEffectIntensity(intensity);
            OnIntensityChanged?.Invoke(intensity);
            
            yield return null;
        }
        
        UpdateEffectIntensity(0f);
        OnIntensityChanged?.Invoke(0f);
    }

    /// <summary>
    /// Updates the effect intensity and applies it to all players
    /// </summary>
    private void UpdateEffectIntensity(float intensity)
    {
        currentIntensity = intensity;
        
        // Update material properties
        if (highlightMaterial != null)
        {
            Color effectiveColor = highlightColor;
            effectiveColor.a *= intensity;
            
            highlightMaterial.SetColor(HighlightColorID, effectiveColor);
            highlightMaterial.SetFloat(PulseSpeedID, pulseSpeed);
            highlightMaterial.SetFloat(EffectIntensityID, intensity);
        }
        
        // Apply to all players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            ApplyIntensityToPlayer(player, intensity);
        }
    }

    /// <summary>
    /// Applies the current intensity to a specific player
    /// </summary>
    private void ApplyIntensityToPlayer(GameObject player, float intensity)
    {
        if (player == null) return;
        
        // Get or add the PredatorVisionEffect component
        PredatorVisionEffect effect = player.GetComponent<PredatorVisionEffect>();
        if (effect == null)
        {
            effect = player.AddComponent<PredatorVisionEffect>();
            effect.Initialize(highlightMaterial);
        }
        
        effect.SetIntensity(intensity);
    }

    /// <summary>
    /// Creates the highlight material
    /// </summary>
    private Material CreateHighlightMaterial()
    {
        Shader highlightShader = Shader.Find("Custom/PlayerRevealHighlight");
        
        if (highlightShader == null)
        {
            highlightShader = Shader.Find("Unlit/Transparent");
            Debug.LogWarning("Custom highlight shader not found, using fallback");
        }

        Material mat = new Material(highlightShader);
        
        if (highlightShader.name.Contains("PlayerRevealHighlight"))
        {
            mat.SetColor(HighlightColorID, highlightColor);
            mat.SetFloat(PulseSpeedID, pulseSpeed);
            mat.SetFloat("_IntensityMin", 0.4f);
            mat.SetFloat("_IntensityMax", 1f);
            mat.SetFloat(EffectIntensityID, 1f);
        }
        else
        {
            mat.color = highlightColor;
        }

        return mat;
    }

    /// <summary>
    /// Plays a sound effect
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Public method to check if reveal is active
    /// </summary>
    public bool IsRevealActive()
    {
        return isRevealActive;
    }

    /// <summary>
    /// Public method to get current intensity
    /// </summary>
    public float GetCurrentIntensity()
    {
        return currentIntensity;
    }

    /// <summary>
    /// Sets the fade durations at runtime
    /// </summary>
    public void SetFadeDurations(float fadeIn, float fadeOut)
    {
        fadeInDuration = Mathf.Max(0.1f, fadeIn);
        fadeOutDuration = Mathf.Max(0.1f, fadeOut);
    }

    private void OnDestroy()
    {
        if (currentRevealCoroutine != null)
        {
            StopCoroutine(currentRevealCoroutine);
        }
        
        // Clean up all player effects
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player != null)
            {
                PredatorVisionEffect effect = player.GetComponent<PredatorVisionEffect>();
                if (effect != null)
                {
                    Destroy(effect);
                }
            }
        }
    }

    // Debug methods for testing
    [ContextMenu("Test Activate")]
    public void TestActivate()
    {
        ActivatePredatorVision(5f);
    }

    [ContextMenu("Test Deactivate")]
    public void TestDeactivate()
    {
        DeactivatePredatorVision();
    }
}