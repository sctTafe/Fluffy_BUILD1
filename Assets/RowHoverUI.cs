using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_RowHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image background;
    private Color normalColor = Color.clear;
    public Color highlightColor = new Color(1f, 1f, 1f, 0.1f);

    private void Start()
    {
        background = GetComponent<Image>();
        normalColor = background.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.color = normalColor;
    }
}
