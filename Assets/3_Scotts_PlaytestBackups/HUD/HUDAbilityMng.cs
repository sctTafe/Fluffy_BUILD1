using System;
using UnityEngine;

public class HUDAbilityMng : MonoBehaviour
{
    [SerializeField] private UI_AbilityDisplayControl _abilityDisplay;

    private IHudAbilityBinder _boundAbility;

    public void BindAbility(IHudAbilityBinder ability)
    {
        if (_boundAbility != null)
        {
            // Unsubscribe previous binding to avoid event leaks
            _boundAbility.OnCooldownWithLengthTriggered -= _abilityDisplay.fn_StartCooldown;
            _boundAbility.OnCooldownCanceled -= _abilityDisplay.fn_CancelCooldown;
        }

        _boundAbility = ability;

        if (_boundAbility != null)
        {
            _boundAbility.OnCooldownWithLengthTriggered += _abilityDisplay.fn_StartCooldown;
            _boundAbility.OnCooldownCanceled += _abilityDisplay.fn_CancelCooldown;
        }
    }

    private void OnDestroy()
    {
        if (_boundAbility != null)
        {
            _boundAbility.OnCooldownWithLengthTriggered -= _abilityDisplay.fn_StartCooldown;
            _boundAbility.OnCooldownCanceled -= _abilityDisplay.fn_CancelCooldown;
        }
    }
}

public interface IHudAbilityBinder
{
    event Action<float> OnCooldownWithLengthTriggered;
    event Action OnCooldownCanceled;

}
