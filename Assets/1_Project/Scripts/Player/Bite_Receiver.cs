using StarterAssets;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Local Client Bite_Receiver
/// 
/// Base on Braedon's, 'GrabPlayer' Script
/// Part of a Two Part System with 'Bite_Activator' & 'Bite_Receiver'
/// </summary>
public class Bite_Receiver : NetworkBehaviour
{
    public UnityEvent OnBiteStart;
    public UnityEvent OnBiteStop;

    [SerializeField] private AnimalCharacter controller;
    [SerializeField] private Vector3 _positionOffset;

    public bool IsGrabbed { get; private set; }
    private bool isGrabbed;
    public float damage = 0.34f;

    public Transform skeleton;
    
    // CharacterController reference instead of Rigidbody and CapsuleCollider
    private CharacterController characterController;
    private Collider[] otherColliders; // For any additional colliders that aren't the CharacterController

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<AnimalCharacter>();    // The Character Controller

        // Get CharacterController reference
        characterController = GetComponent<CharacterController>();
        
        // Get all other colliders (excluding CharacterController's internal collider)
        otherColliders = GetComponentsInChildren<Collider>();
    }

    public void fn_SetBiteMode(bool isBitten, Vector3 pos)
    {
        if (isBitten)
            ActivateBiteModeRpc(pos);
        else
            DisableBiteModeRPC();
    }
  
    [Rpc(SendTo.Everyone)]
    private void ActivateBiteModeRpc(Vector3 pos)
    {
        // Reposition the bite target transform position to that of the bitter
        this.transform.position = pos;
        skeleton.localEulerAngles = new Vector3(0f, 0f, 90f); //set local child rotation
        this.transform.position += _positionOffset;

        // Disable NetworkTransform and movement
        gameObject.GetComponent<NetworkTransform>().enabled = false;
        controller.fn_IsMovementInputDisabled(true);
        
        // Apply damage if this is the owner
        if (IsOwner) 
            gameObject.GetComponent<PlayerHealth>().ChangePlayerHealth(damage);
        
        // Disable CharacterController instead of making Rigidbody kinematic
        if (characterController != null)
            characterController.enabled = false;
        
        // Disable any additional colliders (but keep CharacterController's built-in collider)
        foreach (var collider in otherColliders)
        {
            if (collider != null && !(collider is CharacterController))
                collider.enabled = false;
        }
        
        OnBiteStart?.Invoke();
        isGrabbed = true;
        IsGrabbed = true;
    }

    [Rpc(SendTo.Everyone)]
    private void DisableBiteModeRPC()
    {
        // Re-enable NetworkTransform and movement
        gameObject.GetComponent<NetworkTransform>().enabled = true;
        skeleton.localEulerAngles = new Vector3(0f, 0f, 0f);
        controller.fn_IsMovementInputDisabled(false);
        
        // Re-enable CharacterController
        if (characterController != null)
            characterController.enabled = true;
        
        // Re-enable any additional colliders
        foreach (var collider in otherColliders)
        {
            if (collider != null && !(collider is CharacterController))
                collider.enabled = true;
        }
        
        isGrabbed = false;
        IsGrabbed = false;
        OnBiteStop?.Invoke();
    }
}
