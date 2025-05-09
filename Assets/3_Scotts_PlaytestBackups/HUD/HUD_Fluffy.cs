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

    public override void fn_Bind(HUDPublisher hUDPublisher)
    {
        if (ISDEBUGGING) Debug.Log("HUD_Fluffy: fn_Bind Called");
        Unbind();

        HUDPublisher_Fluffy HPF = (HUDPublisher_Fluffy)hUDPublisher; //Recast to origonal type
        
        _StaminaMng = HPF._StaminaResMng;
        _StaminaMng._OnValueChange += Handle_OnValueChange_Stamina;

        _HealthMng = HPF._HealthResMng;
        _HealthMng._OnValueChange += Handle_OnValueChange_Health;
    }

    private void Unbind()
    {
        if(_StaminaMng != null)
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



    #region Stamina


    #endregion END: Stamina
}
