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

    //Bound Publisher
    HUDPublisher_Mutant _HPF;

    public override void fn_Bind(HUDPublisher hUDPublisher)
    {
        if (ISDEBUGGING) Debug.Log("HUD_Mutant: fn_Bind Called");

        base.fn_Bind(hUDPublisher);

        //_HPF = (HUDPublisher_Mutant)hUDPublisher; //Recast to origonal type
        _HPF = (HUDPublisher_Mutant)_bound_hUDPublisher; //Recast to origonal type

        _MutantEnergyMng = _HPF._MutantResMng;
        _MutantEnergyMng._OnValueChange += Handle_OnValueChange_MutantEnergy;

        TryBindActionButtons();
    }

    protected override void Unbind()
    {
        base.Unbind();

        _HPF = null;
        if (_MutantEnergyMng != null)
        {
            _MutantEnergyMng._OnValueChange -= Handle_OnValueChange_MutantEnergy;
            _MutantEnergyMng = null;
        }
    }

    private void Handle_OnValueChange_MutantEnergy()
    {
        _mutantEnergySlider.fn_SetFillPct_NoLerp(_MutantEnergyMng.fn_GetCurrentPercent());
    }




    
}
