using System;
using UnityEngine;
using UnityEngine.Events;

public class ScottsBackup_PlayerAction_Basic : PlayerActionBase, IHudAbilityBinder
{ 

public event Action<float> OnCooldownWithLengthTriggered;
public event Action OnCooldownCanceled;

public UnityEvent _OnAvilityActivation;



private bool _inputRecived;



void Start()
{
    // Disable Self If Not Owner
    if (!IsOwner)
    {
        this.enabled = false;
        return;
    }

}

void Update()
{
    if (!IsOwner)
        return;

    if (_inputRecived)
    {
        TryAction();
        _inputRecived = false;
    }
    else
    {

    }
}



public override bool fn_ReceiveActivationInput(bool b)
{
    _inputRecived = b;
    OnCooldownWithLengthTriggered?.Invoke(0.1f);
    return false;
}



private void TryAction()
{
        OnCooldownWithLengthTriggered?.Invoke(0.5f);
        _OnAvilityActivation?.Invoke();
}
}
