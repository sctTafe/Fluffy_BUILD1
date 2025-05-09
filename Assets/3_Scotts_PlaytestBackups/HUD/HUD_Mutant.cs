using System;
using UnityEngine;

public class HUD_Mutant : HUD
{
    private const bool ISDEBUGGING = false;

    // MutantEnergy
    #region MutantEnergy
    [SerializeField] UI_SliderOutputControl _mutantEnergySlider;
    ScottsBackup_ResourceMng _MutantEnergyMng;  // MutantEnergy Interaction Data
    #endregion END: MutantEnergy

    public override void fn_Bind(HUDPublisher hUDPublisher)
    {
        if (ISDEBUGGING) Debug.Log("HUD_Mutant: fn_Bind Called");
        Unbind();

        HUDPublisher_Mutant HPF = (HUDPublisher_Mutant)hUDPublisher; //Recast to origonal type

        _MutantEnergyMng = HPF._MutantResMng;
        _MutantEnergyMng._OnValueChange += Handle_OnValueChange_MutantEnergy;
    }

    private void Unbind()
    {
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
