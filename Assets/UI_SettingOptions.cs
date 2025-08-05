using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class OptionChangedEvent : UnityEvent<int, string> { }

public class UI_SettingOptions : MonoBehaviour
{
    [Header("Label (TMP or Text)")]
    public TextMeshProUGUI valueLabelTMP;
    public Text valueLabel;

    [Header("Options")]
    public string[] options;
    [Tooltip("Set the initial index (default 0)")]
    public int initialIndex = 0;

    [Header("Events")]
    public OptionChangedEvent onOptionChanged;

    private int index = 0;
    private bool initialized = false;

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (initialized) return;
        initialized = true;
        if (options == null || options.Length == 0)
            options = new string[] { "Option" };
        index = Mathf.Clamp(initialIndex, 0, options.Length - 1);
        UpdateLabel(true);
    }

    public void SetOptions(string[] newOptions, int startIndex = 0)
    {
        options = newOptions;
        index = Mathf.Clamp(startIndex, 0, options.Length - 1);
        UpdateLabel(true);
    }

    public void Next()
    {
        if (options == null || options.Length == 0) return;
        index = (index + 1) % options.Length;
        UpdateLabel();
    }

    public void Previous()
    {
        if (options == null || options.Length == 0) return;
        index = (index - 1 + options.Length) % options.Length;
        UpdateLabel();
    }

    void UpdateLabel(bool instant = false)
    {
        string val = options[index];
        if (valueLabelTMP != null)
            valueLabelTMP.text = val;
        if (valueLabel != null)
            valueLabel.text = val;
        if (!instant)
            PlayChangeAnimation();
        onOptionChanged?.Invoke(index, val);
    }

    void PlayChangeAnimation()
    {
        // Animate scale punch for TMP or Text label
        if (valueLabelTMP != null)
        {
            valueLabelTMP.transform.DOKill();
            valueLabelTMP.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0.8f);
        }
        else if (valueLabel != null)
        {
            valueLabel.transform.DOKill();
            valueLabel.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0.8f);
        }
    }

    public int GetIndex() => index;
    public string GetValue() => options != null && options.Length > 0 ? options[index] : string.Empty;
}
