using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// Notes: 
///     Hooks into Unitys IPointerEvents
///     Uses DoTween Package/Libary
/// Usage:
///     Expands from the little blue ring thigns in the UI setup
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class OnPointerRectTransfromResize_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform _buttonTrans;
    [SerializeField] private float _scaleMultiplier_Pct = 5f; //5%
    private Outline outline;

    public void Start()
    {
        //DOSE: returns the rectTransform the script is attached to, if 'nuil' (not player set)
        //_buttonTrans ??= GetComponent<RectTransform>();
        if (_buttonTrans == null)
            _buttonTrans = GetComponent<RectTransform>();

        outline = GetComponent<Outline>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _buttonTrans.DOScale((1f+0.01f*_scaleMultiplier_Pct), 0.2f);
        if(outline != null)
        {
            outline.enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _buttonTrans.DOScale(1f, 0.2f);
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}
