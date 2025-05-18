using TMPro;
using UnityEngine;
using DG.Tweening;

public class HUD_PopUpMessages_Singelton : Singleton<HUD_PopUpMessages_Singelton>
{
    public enum PopupStyle
    {
        error,
        Bounce,
        PopAndFade,
        Wobble
    }

    public Canvas canvas;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    public TMP_Text messageText;

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;

    private Sequence currentSequence;
    private void Start()
    {
        canvas.enabled = false;
    }
    private void Reset()
    {
        canvas = GetComponentInChildren<Canvas>();
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        rectTransform = GetComponentInChildren<RectTransform>();
        messageText = GetComponentInChildren<TMP_Text>();
        canvas.enabled = false;
    }

    public void fn_PopupMessage(string message, PopupStyle style, float visibleDuration = 2f)
    {
        messageText.text = message;
        //gameObject.SetActive(true);
        canvas.enabled = true;

        // Reset transform and canvasGroup
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0;

        currentSequence?.Kill();
        currentSequence = DOTween.Sequence();

        switch (style)
        {
            case PopupStyle.Bounce:
                currentSequence.Append(rectTransform.DOScale(1.2f, animationDuration).SetEase(Ease.OutBack))
                               .Join(canvasGroup.DOFade(1f, 0.2f))
                               .AppendInterval(visibleDuration)
                               .Append(rectTransform.DOScale(0.8f, 0.3f).SetEase(Ease.InBack))
                               .Join(canvasGroup.DOFade(0f, 0.3f))
                               .OnComplete(() => gameObject.SetActive(false));
                break;

            case PopupStyle.PopAndFade:
                currentSequence.Append(rectTransform.DOScale(1f, animationDuration).SetEase(Ease.OutElastic))
                               .Join(canvasGroup.DOFade(1f, 0.2f))
                               .AppendInterval(visibleDuration)
                               .Append(canvasGroup.DOFade(0f, 0.5f))
                               .OnComplete(() => gameObject.SetActive(false));
                break;

            case PopupStyle.Wobble:
                currentSequence.Append(rectTransform.DOScale(new Vector3(1.3f, 0.7f, 1f), 0.3f).SetEase(Ease.OutExpo))
                               .Join(canvasGroup.DOFade(1f, 0.3f))
                               .Append(rectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutElastic))
                               .AppendInterval(visibleDuration)
                               .Append(canvasGroup.DOFade(0f, 0.4f))
                               .Join(rectTransform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack))
                               .OnComplete(() => gameObject.SetActive(false));
                break;
        }
        currentSequence.OnComplete(() =>
        {
            //gameObject.SetActive(false);
            canvas.enabled = false;
        });
    }
}
