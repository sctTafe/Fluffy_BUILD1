using System;
using UnityEngine;

public class HUD_Mutant : HUD
{
    private const bool ISDEBUGGING = false;

    // MutantEnergy
    #region MutantEnergy
    [Header("Energy Bars")]
    [SerializeField] UI_SliderOutputControl _mutantEnergySlider;
    ScottsBackup_ResourceMng _MutantEnergyMng;  // MutantEnergy Interaction Data
    #endregion END: MutantEnergy

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
    HUDPublisher_Mutant _HPF;

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
        if (ISDEBUGGING) Debug.Log("HUD_Mutant: fn_Bind Called");
        Unbind();
        

        _HPF = (HUDPublisher_Mutant)hUDPublisher; //Recast to origonal type

        _MutantEnergyMng = _HPF._MutantResMng;
        _MutantEnergyMng._OnValueChange += Handle_OnValueChange_MutantEnergy;

        TryBindActionButtons();
    }

    private void Unbind()
    {
        _HPF = null;
        if (_MutantEnergyMng != null)
        {
            _MutantEnergyMng._OnValueChange -= Handle_OnValueChange_MutantEnergy;
            _MutantEnergyMng = null;
        }
        DisableAllActionButtons();
    }

    private void Handle_OnValueChange_MutantEnergy()
    {
        _mutantEnergySlider.fn_SetFillPct_NoLerp(_MutantEnergyMng.fn_GetCurrentPercent());
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
