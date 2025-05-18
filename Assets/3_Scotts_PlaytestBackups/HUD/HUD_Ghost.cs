using System;
using UnityEngine;

public class HUD_Ghost : HUD
{
    private const bool ISDEBUGGING = false;

    [Header("Energy Bars")]
    // MutantEnergy
    #region GhostEnergy
    [SerializeField] UI_SliderOutputControl _ghostEnergySlider;
    ScottsBackup_ResourceMng _GhostEnergyMng;  // GhostEnergy Interaction Data
    #endregion END: GhostEnergy

    //Bound Publisher
    HUDPublisher_Ghost _HPF;

    public override void fn_Bind(HUDPublisher hUDPublisher)
    {
        if (ISDEBUGGING) Debug.Log("HUD_Ghost: fn_Bind Called");

        base.fn_Bind(hUDPublisher);

        _HPF = (HUDPublisher_Ghost)hUDPublisher; //Recast to origonal type

        _GhostEnergyMng = _HPF._GhostResMng;
        _GhostEnergyMng._OnValueChange += Handle_OnValueChange_GhostEnergy;
     
    }

    protected override void Unbind()
    {
        base.Unbind();

        _HPF = null;
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
