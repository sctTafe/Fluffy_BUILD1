using System;
using UnityEngine;

public class HUDAbilityMng : MonoBehaviour
{
    private const bool ISDEBUGGING = false;

    [SerializeField] private UI_AbilityDisplayControl _abilityDisplay;

    private IHudAbilityBinder _boundAbility;

    private void OnDisable()
    {
        Unbind();
    }
    private void OnDestroy()
    {
        Unbind();
    }

    public void fn_Bind(IHudAbilityBinder ability)
    {
        if (ISDEBUGGING) Debug.Log($"HUDAbilityMng: fn_Bind Called! On {this.gameObject.name}");

        Unbind();

        this.gameObject.SetActive(true);

        _boundAbility = ability;

        if (_boundAbility != null)
        {
            _boundAbility.OnCooldownWithLengthTriggered += _abilityDisplay.fn_StartCooldown;
            _boundAbility.OnCooldownCanceled += _abilityDisplay.fn_CancelCooldown;
        }
    }

    private void Unbind()
    {
        if (_boundAbility != null)
        {
            _boundAbility.OnCooldownWithLengthTriggered -= _abilityDisplay.fn_StartCooldown;
            _boundAbility.OnCooldownCanceled -= _abilityDisplay.fn_CancelCooldown;
            _boundAbility = null;
        }
    }


}

public interface IHudAbilityBinder
{
    event Action<float> OnCooldownWithLengthTriggered;
    event Action OnCooldownCanceled;

}
