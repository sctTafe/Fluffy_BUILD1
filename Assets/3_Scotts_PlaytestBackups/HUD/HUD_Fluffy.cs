using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// NOTE: Update constantly running atm, should change it to event driven
/// 
/// </summary>
public class HUD_Fluffy : HUD
{
    // Stamina
    #region Stamina
    [SerializeField] UI_SliderOutputControl _staminaSlider;
    FluffyPlayerDataManager_Local _LocalPlayerData;  // Stamina Interaction Data
    #endregion END: Stamina

    // Health
    #region Stamina
    [SerializeField] UI_SliderOutputControl _healthSlider;
    PlayerHealth _localPlayerHealth;


    #endregion END: Stamina

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    void OnEnable()
    {

    }

    public override void fn_Bind(HUDPublisher hUDPublisher)
    {

    }



    #region Stamina


    #endregion END: Stamina
}
