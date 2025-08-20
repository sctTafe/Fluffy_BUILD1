using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Component that handles predator vision highlighting for individual players
/// Automatically added to players when the effect is activated
/// </summary>
public class PredatorVisionEffect : MonoBehaviour
{
    private Material highlightMaterial;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private bool isEffectActive = false;
    private float currentIntensity = 0f;
    
    // Cache of all renderers for performance
    private Renderer[] allRenderers;
    
    /// <summary>
    /// Initialize the effect with the highlight material
    /// </summary>
    public void Initialize(Material material)
    {
        highlightMaterial = material;
        allRenderers = GetComponentsInChildren<Renderer>();
        
        // Store original materials
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null)
            {
                originalMaterials[renderer] = renderer.materials;
            }
        }
    }

    /// <summary>
    /// Set the intensity of the effect (0 = off, 1 = full intensity)
    /// </summary>
    public void SetIntensity(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);
        currentIntensity = intensity;
        
        if (intensity > 0f && !isEffectActive)
        {
            EnableEffect();
        }
        else if (intensity <= 0f && isEffectActive)
        {
            DisableEffect();
        }
        
        UpdateMaterialIntensity(intensity);
    }

    /// <summary>
    /// Enable the highlighting effect
    /// </summary>
    private void EnableEffect()
    {
        if (isEffectActive || highlightMaterial == null) return;
        
        isEffectActive = true;
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && originalMaterials.ContainsKey(renderer))
            {
                ApplyHighlightToRenderer(renderer);
            }
        }
    }

    /// <summary>
    /// Disable the highlighting effect
    /// </summary>
    private void DisableEffect()
    {
        if (!isEffectActive) return;
        
        isEffectActive = false;
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && originalMaterials.ContainsKey(renderer))
            {
                RestoreOriginalMaterials(renderer);
            }
        }
    }

    /// <summary>
    /// Apply highlight material to a specific renderer
    /// </summary>
    private void ApplyHighlightToRenderer(Renderer renderer)
    {
        if (renderer == null || !originalMaterials.ContainsKey(renderer)) return;
        
        Material[] original = originalMaterials[renderer];
        Material[] newMaterials = new Material[original.Length + 1];
        
        // Copy original materials
        for (int i = 0; i < original.Length; i++)
        {
            newMaterials[i] = original[i];
        }
        
        // Add highlight material as the last material
        newMaterials[newMaterials.Length - 1] = highlightMaterial;
        
        renderer.materials = newMaterials;
    }

    /// <summary>
    /// Restore original materials to a specific renderer
    /// </summary>
    private void RestoreOriginalMaterials(Renderer renderer)
    {
        if (renderer != null && originalMaterials.ContainsKey(renderer))
        {
            renderer.materials = originalMaterials[renderer];
        }
    }

    /// <summary>
    /// Update the intensity of the highlight material
    /// </summary>
    private void UpdateMaterialIntensity(float intensity)
    {
        if (highlightMaterial != null)
        {
            if (highlightMaterial.HasProperty("_EffectIntensity"))
            {
                highlightMaterial.SetFloat("_EffectIntensity", intensity);
            }
            
            // Also update alpha channel of highlight color
            if (highlightMaterial.HasProperty("_HighlightColor"))
            {
                Color currentColor = highlightMaterial.GetColor("_HighlightColor");
                currentColor.a = intensity;
                highlightMaterial.SetColor("_HighlightColor", currentColor);
            }
        }
    }

    /// <summary>
    /// Check if the effect is currently active
    /// </summary>
    public bool IsEffectActive()
    {
        return isEffectActive;
    }

    /// <summary>
    /// Get the current intensity
    /// </summary>
    public float GetCurrentIntensity()
    {
        return currentIntensity;
    }

    /// <summary>
    /// Force cleanup of the effect
    /// </summary>
    public void ForceCleanup()
    {
        DisableEffect();
        originalMaterials.Clear();
    }

    /// <summary>
    /// Handle component being added to a new player at runtime
    /// </summary>
    private void Start()
    {
        if (allRenderers == null || allRenderers.Length == 0)
        {
            allRenderers = GetComponentsInChildren<Renderer>();
            
            // Store original materials if not already done
            foreach (Renderer renderer in allRenderers)
            {
                if (renderer != null && !originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.materials;
                }
            }
        }
    }

    /// <summary>
    /// Clean up when component is destroyed
    /// </summary>
    private void OnDestroy()
    {
        ForceCleanup();
    }

    /// <summary>
    /// Update the effect when new renderers are added (e.g., equipment changes)
    /// </summary>
    public void RefreshRenderers()
    {
        // Store current state
        bool wasActive = isEffectActive;
        float storedIntensity = currentIntensity;
        
        // Cleanup current effect
        DisableEffect();
        
        // Refresh renderer list
        allRenderers = GetComponentsInChildren<Renderer>();
        
        // Clear and rebuild original materials dictionary
        originalMaterials.Clear();
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null)
            {
                originalMaterials[renderer] = renderer.materials;
            }
        }
        
        // Restore previous state
        if (wasActive)
        {
            SetIntensity(storedIntensity);
        }
    }

    // Debug methods
    [ContextMenu("Test Enable Effect")]
    public void TestEnableEffect()
    {
        SetIntensity(1f);
    }

    [ContextMenu("Test Disable Effect")]
    public void TestDisableEffect()
    {
        SetIntensity(0f);
    }

    [ContextMenu("Refresh Renderers")]
    public void TestRefreshRenderers()
    {
        RefreshRenderers();
    }
}