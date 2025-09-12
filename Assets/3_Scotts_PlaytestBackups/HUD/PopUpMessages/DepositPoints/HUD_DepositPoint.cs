using DG.Tweening;
using TMPro;
using UnityEngine;

public class HUD_DepositPoint : MonoBehaviour
{
    [SerializeField] DepositItemPoint _dp;

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
    private float nextAvailableTime = 0f; // cooldown timer

    private void Start()
    {
        canvas.enabled = false;

        _dp._OnDepositItem += Handle_OnDepositItem;
    }
    private void OnDisable()
    {
        _dp._OnDepositItem -= Handle_OnDepositItem;
    }

    private void Handle_OnDepositItem()
    {
        fn_CancelPopup(); // just is a work around, for not showing a version with the wrong number when a deposit is made.  it will cancel the message if any player deposits, -> minor bug
    }

    private void Reset()
    {
        canvas = GetComponentInChildren<Canvas>();
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        rectTransform = GetComponentInChildren<RectTransform>();
        messageText = GetComponentInChildren<TMP_Text>();
        canvas.enabled = false;
    }

    private void fn_PopupMsg_TEMP(string msg)
    {

        fn_PopupMessage("Bounce Message", HUD_DepositPoint.PopupStyle.Bounce, 2f);


        fn_PopupMessage("Pop & Fade Message", HUD_DepositPoint.PopupStyle.PopAndFade, 3f);


        fn_PopupMessage("Wobble Message", HUD_DepositPoint.PopupStyle.Wobble, 1.5f);
    }
    public void fn_PopupMsg_PopAndFade()
    {
        fn_PopupMessage($"{_dp.current_amount.Value} / {_dp.amount_needed} Collected", HUD_DepositPoint.PopupStyle.PopAndFade, 3f);
    }


    public void fn_CancelPopup()
    {
        currentSequence?.Kill();
        nextAvailableTime = Time.time + 0.5f;
        canvas.enabled = false;
    }

    private void fn_PopupMessage(string message, PopupStyle style, float visibleDuration = 2f, bool overideCooldown = false)
    {
        if (!overideCooldown)
        {
            // Check cooldown
            if (Time.time < nextAvailableTime)
                return;
        }

        // Set next available time (visibleDuration + 2f buffer)
        nextAvailableTime = Time.time + visibleDuration + 2f;

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
            canvas.enabled = false;
        });
    }
}
