using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

// Make sure to match the class name exactly as in UI_SettingOptions
public class GraphicsQualityMenu : MonoBehaviour
{
    [Header("Preset Selector")]
    public UI_SettingOptions presetSelector;

    [Header("Advanced Options (Optional)")]
    public UI_SettingOptions shadowQualitySelector;
    public UI_SettingOptions textureQualitySelector;
    // Add more advanced selectors as needed

    private readonly string[] presetNames = { "Low", "Medium", "High", "Custom" };
    private bool ignoreEvents = false;

    void Start()
    {
        // Setup preset selector
        presetSelector.SetOptions(presetNames, QualitySettings.GetQualityLevel());
        presetSelector.onOptionChanged.AddListener(OnPresetChanged);

        // Setup advanced selectors if present
        if (shadowQualitySelector != null)
        {
            shadowQualitySelector.SetOptions(new[] { "Low", "Medium", "High" }, 2 - QualitySettings.shadowCascades / 2);
            shadowQualitySelector.onOptionChanged.AddListener(OnShadowChanged);
        }
        if (textureQualitySelector != null)
        {
            textureQualitySelector.SetOptions(new[] { "Full Res", "Half Res", "Quarter Res" }, QualitySettings.globalTextureMipmapLimit);
            textureQualitySelector.onOptionChanged.AddListener(OnTextureChanged);
        }
    }

    void OnPresetChanged(int index, string name)
    {
        if (ignoreEvents) return;
        if (index < 3) // Low, Medium, High
        {
            QualitySettings.SetQualityLevel(index, true);
            ignoreEvents = true;
            UpdateAdvancedSelectorsFromQuality(index);
            ignoreEvents = false;
        }
    }

    void OnShadowChanged(int index, string name)
    {
        if (ignoreEvents) return;
        int[] cascades = { 0, 2, 4 };
        QualitySettings.shadowCascades = cascades[index];
        SetPresetToCustom();
    }

    void OnTextureChanged(int index, string name)
    {
        if (ignoreEvents) return;
        QualitySettings.globalTextureMipmapLimit = index;
        SetPresetToCustom();
    }

    void SetPresetToCustom()
    {
        if (presetSelector.GetValue() != "Custom")
        {
            ignoreEvents = true;
            presetSelector.SetOptions(presetNames, 3); // Set to Custom
            ignoreEvents = false;
        }
    }

    void UpdateAdvancedSelectorsFromQuality(int qualityLevel)
    {
        if (shadowQualitySelector != null)
        {
            int shadowIndex = Mathf.Clamp(2 - QualitySettings.shadowCascades / 2, 0, 2);
            shadowQualitySelector.SetOptions(new[] { "Low", "Medium", "High" }, shadowIndex);
        }
        if (textureQualitySelector != null)
        {
            int textureIndex = Mathf.Clamp(QualitySettings.globalTextureMipmapLimit, 0, 2);
            textureQualitySelector.SetOptions(new[] { "Full Res", "Half Res", "Quarter Res" }, textureIndex);
        }
    }
}
