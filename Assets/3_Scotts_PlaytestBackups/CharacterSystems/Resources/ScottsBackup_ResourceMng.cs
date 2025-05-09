using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Author:
///     Scott Barley  07/05/25
/// IS:
///     Class for handling resource management
///     Expected Use: Stamina & Energy
/// </summary>
public class ScottsBackup_ResourceMng : NetworkBehaviour
{
    private const bool ISDEBUGGING = false;

    public enum ResourceType
    {
        error,
        Stamina,
        Health,
        MutantEnergy,
        Ghost
    }

    public Action _OnExhaustionTriggered;
    public Action _OnValueChange;
    public Action<bool> _OnValueChangeUp;


    [Header("Resource Settings")]
    [SerializeField] private string _resourceID;
    [SerializeField] private ResourceType _resourceTypeID;
    public ResourceType ResourceTypeID { get { return _resourceTypeID; } }

    // on Start
    [SerializeField] private bool isStartingAtMax;

    // Resource Recharge
    [Header("Resource Recharge")]
    [SerializeField] public bool isAutoRechargeEnabled;
    private bool isRechargePaused;
    private float rechargeDelayTimer;
    [SerializeField] public float rechargeDelaySecond = 1f;
    [SerializeField] public float valueRechargePerSecond = 10f;

    // Resource Exhaustion
    [Header("Resource Exhaustion")]
    private bool isExhausted;
    private float exhaustionTimer;
    public UnityEvent OnExhaustionTriggered_UE;
    [SerializeField] public float exhaustedCooldown = 2f;            // Stop Stamina Usage for 2s if the player becomes exhusted



    // Soft min value
    private float softMin = 2; // Can go under 0, soft cap

    // basic
    private float _maxValue = 100f;
    private float _currentValue;



    #region Unity Native
    void Start()
    {
        // Disable Self If Not Owner
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }

        if(isStartingAtMax)
            _currentValue = _maxValue;

        if (_resourceTypeID == ResourceType.error)
            Debug.LogWarning("ScottsBackup_ResourceMng: Resource Type Not Set!");
    }

    void Update() 
    {
        if (!IsOwner)
            return;
        
        // cooldowns
        Update_RechargeCooldown();
        Update_ExhaustionCooldown();
        // resoruce recharge
        Update_Recharge();
    }
    #endregion

    #region Public Functions
    public float fn_GetCurrentPercent()
    {
        return _currentValue / _maxValue;
    }

    public float fn_GetCurrentValue()
    {
        return _currentValue;
    }

    public bool fn_TryReduceValue(float amount)
    {
        // fail
        if (amount < 0.00)
            return false;

        if(IsExhausted())
            return false;

        
        if (amount > (_currentValue + softMin))
        {
            return false;
        }

        // ok
        _currentValue -= amount;
        Trigger_RechargePaused();
        CheckForExhaustion();
        _OnValueChange?.Invoke();
        _OnValueChangeUp?.Invoke(false);

        if (ISDEBUGGING) Debug.Log($"ScottsBackup_ResourceMng: Amount Reduced {amount}");

        return true;
    }

    public void fn_TryIncreaseValue(float amount)
    {
        // fail
        if (amount < 0)
            return;

        // ok
        _currentValue += amount;
        _currentValue = Mathf.Min(_currentValue, _maxValue);
        _OnValueChange?.Invoke();
        _OnValueChangeUp?.Invoke(true);

        if (ISDEBUGGING) Debug.Log($"ScottsBackup_ResourceMng: Amount Increased {amount}");
    }   
    #endregion


    private void CheckForExhaustion()
    {
        if(_currentValue <= 0)
        {
            _currentValue = 0;
            Trigger_Exhausted();
            _OnExhaustionTriggered?.Invoke();
            OnExhaustionTriggered_UE?.Invoke();
        }
    }

    private void Update_Recharge()
    {
        if(!isAutoRechargeEnabled)
            return;

        if (IsRechargePaused())
            return;

        _currentValue += valueRechargePerSecond * Time.deltaTime;
        _currentValue = Mathf.Min(_currentValue, _maxValue);
        _OnValueChange?.Invoke();
        _OnValueChangeUp?.Invoke(true);
    }


    #region Timers
    // --- Recharge Delay ---
    private bool IsRechargePaused() => isRechargePaused;
    private void Trigger_RechargePaused()
    {
        rechargeDelayTimer = Time.time + rechargeDelaySecond;
        isRechargePaused = true;
    }
    private void Update_RechargeCooldown()
    {
        if (!isRechargePaused)
            return;

        if (rechargeDelayTimer <= Time.time)
        {
            isRechargePaused = false;
        }
    }

    // --- Exhaustion Timer ---
    private bool IsExhausted() => isExhausted;

    private void Trigger_Exhausted()
    {
        exhaustionTimer = Time.time + exhaustedCooldown;
        isExhausted = true;
    }

    private void Update_ExhaustionCooldown()
    {
        if (!isExhausted)
            return;

        if (exhaustionTimer <= Time.time)
        {
            isExhausted = false;
        }
    }
    #endregion

}
