using System;
using UnityEngine;

public class HUDAbilityMng : MonoBehaviour
{
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
        Unbind();

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
