using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

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

    public void Start()
    {
        //DOSE: returns the rectTransform the script is attached to, if 'nuil' (not player set)
        //_buttonTrans ??= GetComponent<RectTransform>();
        if (_buttonTrans == null)
            _buttonTrans = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _buttonTrans.DOScale((1f+0.01f*_scaleMultiplier_Pct), 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _buttonTrans.DOScale(1f, 0.2f);
    }
}
