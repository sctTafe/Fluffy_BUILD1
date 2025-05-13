using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:
///     Scott Barley  07/05/25
/// IS:
///     Handles HUD/UI Output for Fluffies
/// </summary>
public class HUD_Fluffy : HUD
{
    private const bool ISDEBUGGING = false;

    [Header("Energy Bars")]
    // Stamina
    #region Stamina
    [SerializeField] UI_SliderOutputControl _staminaSlider;
    ScottsBackup_ResourceMng _StaminaMng;  // Stamina Interaction Data
    #endregion END: Stamina

    // Health
    #region Stamina
    [SerializeField] UI_SliderOutputControl _healthSlider;
    ScottsBackup_ResourceMng _HealthMng;
    #endregion END: Stamina

    //Bound Publisher
    HUDPublisher_Fluffy _HPF;

    public override void fn_Bind(HUDPublisher hUDPublisher)
    {
        if (ISDEBUGGING) Debug.Log("HUD_Fluffy: fn_Bind Called");
        
        base.fn_Bind(hUDPublisher);

        _HPF = (HUDPublisher_Fluffy)_bound_hUDPublisher; //Recast to origonal type
        
        _StaminaMng = _HPF._StaminaResMng;
        _StaminaMng._OnValueChange += Handle_OnValueChange_Stamina;

        _HealthMng = _HPF._HealthResMng;
        _HealthMng._OnValueChange += Handle_OnValueChange_Health;
        
    }

    protected override void Unbind()
    {
        base.Unbind();

        _HPF = null;
        if (_StaminaMng != null)
        {
            _StaminaMng._OnValueChange -= Handle_OnValueChange_Stamina;
            _StaminaMng = null;
        }

        if (_HealthMng != null)
        {
            _HealthMng._OnValueChange -= Handle_OnValueChange_Health;
            _HealthMng = null;
        }

    }

    private void Handle_OnValueChange_Stamina()
    {
        _staminaSlider.fn_SetFillPct_NoLerp(_StaminaMng.fn_GetCurrentPercent());
    }

    private void Handle_OnValueChange_Health()
    {
        _healthSlider.fn_SetFillPct_NoLerp(_HealthMng.fn_GetCurrentPercent());
    }
}
