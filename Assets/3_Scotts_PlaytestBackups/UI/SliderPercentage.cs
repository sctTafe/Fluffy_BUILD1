using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderValueDisplay : MonoBehaviour
{
    public enum DisplayMode
    {
        Percentage,
        RawValue
    }

    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private DisplayMode displayMode = DisplayMode.Percentage;

    private void Start()
    {
        UpdateDisplay(slider.value);
        slider.onValueChanged.AddListener(UpdateDisplay);
    }

    private void UpdateDisplay(float value)
    {
        switch (displayMode)
        {
            case DisplayMode.Percentage:
                int percent = Mathf.RoundToInt(value * 100f);
                valueText.text = percent + "%";
                break;

            case DisplayMode.RawValue:
                // Example: 2 decimal places
                valueText.text = value.ToString("0.00");
                break;
        }
    }
}
