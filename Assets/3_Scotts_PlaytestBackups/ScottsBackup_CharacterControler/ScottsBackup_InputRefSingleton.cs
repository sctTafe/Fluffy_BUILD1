using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Author: Scott Barley 07/05/2025
/// DOSE: Singleton Ref for Local player inputs, allows player network prefabs to connect to the local inputs
/// </summary>
[RequireComponent(typeof(ScottsBackupInputSystem))]
public class ScottsBackup_InputRefSingleton : Singleton<ScottsBackup_InputRefSingleton>
{
    public ScottsBackupInputSystem _inputs;
    public PlayerInput _playerInput;

    private void Awake()
    {
        _inputs = GetComponent<ScottsBackupInputSystem>();
        _playerInput = GetComponent<PlayerInput>();
    }
}
