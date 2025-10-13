using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class OnPointerOver_TMPColourChange_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI[] targetTexts; // Assign your child TMPs here

    [Header("Colour Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    // Called when mouse enters the panel area
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetTextColors(hoverColor);
    }

    // Called when mouse exits the panel area
    public void OnPointerExit(PointerEventData eventData)
    {
        SetTextColors(normalColor);
    }

    private void SetTextColors(Color color)
    {
        if (targetTexts == null) return;

        foreach (var tmp in targetTexts)
        {
            if (tmp != null)
                tmp.color = color;
        }
    }
}