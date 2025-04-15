using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.TextCore.Text;


public class PlayerController : NetworkBehaviour
{
    public CharacterBase _currentCharacter;
    private PlayerInputs _inputActions;
    private Vector2 _movementInput;

    [Header("Events")]
    public UnityEvent Landed = new UnityEvent();

    private void Awake()
    {
        _inputActions = new PlayerInputs();
        _inputActions.Player.Move.performed += OnMovePerformed;
        _inputActions.Player.Move.canceled += OnMoveCanceled;
        
    }

    private void Start()
    {
        // Assign aim core target if needed.
        //if (IsOwner)
            //PlayerAimCoreLinker_Singleton.Instance.AssignAimCoreTarget(_currentCharacter as AnimalCharacter);

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Only enable input if this is the local (owner) player.
        if (!IsOwner)
        {
            _inputActions.Player.Disable();
        }
        else
        {
            _inputActions.Player.Enable();
        }
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void FixedUpdate()
    {
        if (!IsOwner || _currentCharacter == null)
            return;

        _currentCharacter.HandleMovement(_movementInput);

        if (_inputActions.Player.Attack.IsPressed())
            _currentCharacter.Attack();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected. Cleaning up player object.");
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
            {
                client.PlayerObject.Despawn();
            }
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        _movementInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _movementInput = Vector2.zero;
    }

}
