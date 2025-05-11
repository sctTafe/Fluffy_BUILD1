using UnityEngine;
using UnityEngine.UI;

public class UI_AbilityDisplayControl : MonoBehaviour
{
    [Header("Cooldown Settings")]
    [Tooltip("GameObject used to mask the icon during cooldown.")]
    public GameObject _maskObject;

    [Tooltip("Optional Image component to fill according to cooldown progress.")]
    public Image _fillImage;

    public float CooldownPercent { get; private set; } = 0f;

    private float cooldownDuration;
    private float cooldownTimeRemaining;
    private bool isOnCooldown = false;

    private void Start()
    {
        EndCooldown();
    }


    void Update()
    {
        if (!isOnCooldown)
            return;

        cooldownTimeRemaining -= Time.deltaTime;

        if (cooldownTimeRemaining <= 0f)
        {
            EndCooldown();
        }
        else
        {
            CooldownPercent = cooldownTimeRemaining / cooldownDuration;
            UpdateVisuals(CooldownPercent);
        }
    }

    /// <summary>
    /// Starts the cooldown for a given duration in seconds.
    /// </summary>
    public void fn_StartCooldown(float duration)
    {
        cooldownDuration = duration;
        cooldownTimeRemaining = duration;
        isOnCooldown = true;
        _maskObject.SetActive(true);
        CooldownPercent = 1f;
        UpdateVisuals(CooldownPercent);
    }

    /// <summary>
    /// Cancels the cooldown early.
    /// </summary>
    public void fn_CancelCooldown()
    {
        EndCooldown();
    }

    private void EndCooldown()
    {
        isOnCooldown = false;
        cooldownTimeRemaining = 0f;
        CooldownPercent = 0f;
        _maskObject.SetActive(false);
        UpdateVisuals(0f);
    }

    private void UpdateVisuals(float percent)
    {
        if (_fillImage != null)
        {
            _fillImage.fillAmount = percent;
        }
    }

    public void fn_Test_2s()
    {
        fn_StartCooldown(2f);
    }
}
