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

    [Header("Action Buttons")]
    [SerializeField] GameObject _ActionUIElement_Main;
    [SerializeField] GameObject _ActionUIElement_Sprint;
    [SerializeField] GameObject _ActionUIElement_1;
    [SerializeField] GameObject _ActionUIElement_2;
    [SerializeField] GameObject _ActionUIElement_3;
    [SerializeField] GameObject _ActionUIElement_4;

    HUDAbilityMng _hUDAbilityMng_Main;
    HUDAbilityMng _hUDAbilityMng_Sprint;
    HUDAbilityMng _hUDAbilityMng_1;
    HUDAbilityMng _hUDAbilityMng_2;
    HUDAbilityMng _hUDAbilityMng_3;
    HUDAbilityMng _hUDAbilityMng_4;

    //Bound Publisher
    HUDPublisher_Fluffy _HPF;

    private void OnDisable()
    {
        Unbind();
    }
    private void OnDestroy()
    {
        Unbind();
    }

    public override void fn_Bind(HUDPublisher hUDPublisher)
    {
        if (ISDEBUGGING) Debug.Log("HUD_Fluffy: fn_Bind Called");
        Unbind();

        _HPF = (HUDPublisher_Fluffy)hUDPublisher; //Recast to origonal type
        
        _StaminaMng = _HPF._StaminaResMng;
        _StaminaMng._OnValueChange += Handle_OnValueChange_Stamina;

        _HealthMng = _HPF._HealthResMng;
        _HealthMng._OnValueChange += Handle_OnValueChange_Health;

        TryBindActionButtons();
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

        DisableAllActionButtons();
    }

    private void Handle_OnValueChange_Stamina()
    {
        _staminaSlider.fn_SetFillPct_NoLerp(_StaminaMng.fn_GetCurrentPercent());
    }

    private void Handle_OnValueChange_Health()
    {
        _healthSlider.fn_SetFillPct_NoLerp(_HealthMng.fn_GetCurrentPercent());
    }


    private void DisableAllActionButtons()
    {
        if (_ActionUIElement_Main != null)
            _ActionUIElement_Main.SetActive(false);

        if (_ActionUIElement_Sprint != null)
            _ActionUIElement_Sprint.SetActive(false);

        if (_ActionUIElement_1 != null)
            _ActionUIElement_1.SetActive(false);

        if (_ActionUIElement_2 != null)
            _ActionUIElement_2.SetActive(false);

        if (_ActionUIElement_3 != null)
            _ActionUIElement_3.SetActive(false);

        if (_ActionUIElement_4 != null)
            _ActionUIElement_4.SetActive(false);

    }

    private void TryBindActionButtons()
    {
        TryBindButton(_HPF.InputMain, _ActionUIElement_Main, _hUDAbilityMng_Main);
        TryBindButton(_HPF.InputSprint, _ActionUIElement_Sprint, _hUDAbilityMng_Sprint);
        TryBindButton(_HPF.ActionInteraction1, _ActionUIElement_1, _hUDAbilityMng_1);
        TryBindButton(_HPF.ActionInteraction2, _ActionUIElement_2, _hUDAbilityMng_2);
        TryBindButton(_HPF.ActionInteraction3, _ActionUIElement_3, _hUDAbilityMng_3);
        TryBindButton(_HPF.ActionInteraction4, _ActionUIElement_4, _hUDAbilityMng_4);

        void TryBindButton(PlayerActionBase action, GameObject uiElement, HUDAbilityMng abilityMng)
        {
            abilityMng = uiElement.GetComponent<HUDAbilityMng>();

            var binder = action as IHudAbilityBinder;
            if (binder != null)
            {
                uiElement.SetActive(true);
                abilityMng.fn_Bind(binder);
            }
            else
            {
                uiElement.SetActive(false);
            }
        }
    }
}
