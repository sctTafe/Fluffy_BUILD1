using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FMODButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable)
            UISoundPlayer.PlayHoverSound();
        else
            UISoundPlayer.PlayDisabledSound();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button.interactable)
            UISoundPlayer.PlayClickSound();
        else
            UISoundPlayer.PlayDisabledSound();
    }
}
