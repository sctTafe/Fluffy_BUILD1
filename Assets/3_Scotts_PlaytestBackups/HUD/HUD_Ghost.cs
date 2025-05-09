using System;
using UnityEngine;

public class HUD_Ghost : HUD
{
    private const bool ISDEBUGGING = false;

    // MutantEnergy
    #region GhostEnergy
    [SerializeField] UI_SliderOutputControl _ghostEnergySlider;
    ScottsBackup_ResourceMng _GhostEnergyMng;  // GhostEnergy Interaction Data
    #endregion END: GhostEnergy

    public override void fn_Bind(HUDPublisher hUDPublisher)
    {
        if (ISDEBUGGING) Debug.Log("HUD_Ghost: fn_Bind Called");
        Unbind();

        HUDPublisher_Ghost HPF = (HUDPublisher_Ghost)hUDPublisher; //Recast to origonal type

        _GhostEnergyMng = HPF._GhostResMng;
        _GhostEnergyMng._OnValueChange += Handle_OnValueChange_GhostEnergy;
    }

    private void Unbind()
    {
        if (_GhostEnergyMng != null)
        {
            _GhostEnergyMng._OnValueChange -= Handle_OnValueChange_GhostEnergy;
            _GhostEnergyMng = null;
        }


    }

    private void Handle_OnValueChange_GhostEnergy()
    {
        _ghostEnergySlider.fn_SetFillPct_NoLerp(_GhostEnergyMng.fn_GetCurrentPercent());
    }
}
