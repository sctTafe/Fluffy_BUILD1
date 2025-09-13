using Unity.Netcode;
using UnityEngine;


// State enums - each uses only 1 byte
public enum MovementState : byte
{
    Idle = 0,
    Walking = 1,
    Running = 2,
    Crouching = 3,
    Jumping = 4,
    Floating = 5,
    Landing = 6
}

public enum ActionState : byte
{
    None = 0,
    Attacking = 1,
    Breathing = 2,
    Revealing = 3,
    Biting = 4,
    BeingBitten = 5,
    Interacting = 6,
    Climbing = 7
}

public class CharacterAnimator : NetworkBehaviour
{
    private Animator animator;

    [Header("Animation Parameters")]
    [SerializeField] private string movementStateParam = "MovementState";
    [SerializeField] private string actionStateParam = "ActionState";

    [Header("Network Optimization")]
    [SerializeField] private float stateChangeThreshold = 0.2f;
    [SerializeField] private float updateCooldown = 0.1f; // 10Hz max updates

    // Network state variables - single byte each!
    public NetworkVariable<MovementState> networkMovementState = new NetworkVariable<MovementState>(
        MovementState.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    public NetworkVariable<ActionState> networkActionState = new NetworkVariable<ActionState>(
        ActionState.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Local state tracking
    private MovementState currentMovementState = MovementState.Idle;
    private ActionState currentActionState = ActionState.None;
    private float lastUpdateTime;
    
    // Action state timing
    private float actionStateStartTime;
    private float actionStateDuration = 2f; // Auto-clear actions after 2 seconds
    
    [Header("Action State Auto-Clear")]
    [SerializeField] private bool autoActionStateClear = false; // Disabled since we use one-shot triggers
    [SerializeField] private float actionStateTimeout = 0.2f; // Much shorter timeout for trigger-like behavior

    // Movement state override system
    private bool hasMovementStateOverride = false;
    private MovementState movementStateOverride = MovementState.Idle;
    private float movementStateOverrideTime = 0f;
    private float movementStateOverrideDuration = 0.1f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to network state changes
        networkMovementState.OnValueChanged += OnMovementStateChanged;
        networkActionState.OnValueChanged += OnActionStateChanged;

        // Initialize with current network values
        if (animator != null)
        {
            ApplyAnimationState(networkMovementState.Value, networkActionState.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        networkMovementState.OnValueChanged -= OnMovementStateChanged;
        networkActionState.OnValueChanged -= OnActionStateChanged;
        base.OnNetworkDespawn();
    }

    /// <summary>
    /// Main update function - call this from your movement controller
    /// </summary>
    public void UpdateAnimatorState(Vector3 velocity, Transform characterTransform, bool isOwner, bool isRunning = false)
    {
        if (!animator || !IsSpawned) return;

        if (isOwner)
        {
            // Owner calculates and broadcasts state
            UpdateOwnerState(velocity, characterTransform, isRunning);
            
            // Handle automatic action state clearing
            if (autoActionStateClear && currentActionState != ActionState.None)
            {
                if (Time.time - actionStateStartTime > actionStateTimeout)
                {
                    if (IsOwner)
                    {
                        Debug.Log($"CharacterAnimator: Auto-clearing action state {currentActionState} after {actionStateTimeout}s");
                        networkActionState.Value = ActionState.None;
                    }
                }
            }
        }
        
        // All clients apply the current network state
        ApplyCurrentNetworkState();
    }

    #region Backward Compatibility Methods

    /// <summary>
    /// Backward compatibility for old UpdateAnimatorLocomotion calls
    /// </summary>
    public void UpdateAnimatorLocomotion(Vector3 velocity, Transform characterTransform, bool isOwner, float networkSideways, float networkForward)
    {
        // Convert old float-based system to new state-based system
        bool isRunning = velocity.magnitude > 3f; // Estimate if running based on speed
        UpdateAnimatorState(velocity, characterTransform, isOwner, isRunning);
    }

    /// <summary>
    /// Backward compatibility for old UpdateJumpState calls
    /// </summary>
    public void UpdateJumpState(int jumpState)
    {
        // Convert old jump state to new movement state
        MovementState newMovementState = MovementState.Idle;
        
        switch (jumpState)
        {
            case 0: // Normal locomotion
                newMovementState = MovementState.Idle;
                break;
            case 1: // Jump start
                newMovementState = MovementState.Jumping;
                break;
            case 2: // Float (loops)
                newMovementState = MovementState.Floating;
                break;
            case 3: // Land
                newMovementState = MovementState.Landing;
                break;
        }

        // Update movement state if owner
        if (IsOwner && currentMovementState != newMovementState)
        {
            networkMovementState.Value = newMovementState;
        }
    }

    #endregion

    private void UpdateOwnerState(Vector3 velocity, Transform characterTransform, bool isRunning)
    {
        // Only update if enough time has passed (rate limiting)
        if (Time.time - lastUpdateTime < updateCooldown) return;

        // Check if we have a movement state override (for jump states)
        if (hasMovementStateOverride && Time.time - movementStateOverrideTime < movementStateOverrideDuration)
        {
            // Use the override state instead of calculated state
            if (currentMovementState != movementStateOverride)
            {
                currentMovementState = movementStateOverride;
                networkMovementState.Value = currentMovementState;
                lastUpdateTime = Time.time;
            }
            return;
        }
        else
        {
            // Clear override if expired
            hasMovementStateOverride = false;
        }

        // Calculate local velocity
        Vector3 localVelocity = characterTransform.InverseTransformDirection(velocity);
        float speed = new Vector2(localVelocity.x, localVelocity.z).magnitude;

        // Determine movement state (only if no override)
        MovementState newMovementState = CalculateMovementState(speed, isRunning);

        // Check if movement state actually changed
        bool movementChanged = newMovementState != currentMovementState;

        if (movementChanged)
        {
            currentMovementState = newMovementState;
            
            // Update network variables (only owner can write)
            networkMovementState.Value = currentMovementState;
            
            lastUpdateTime = Time.time;
        }
        
        // Action state is handled separately via public methods - don't interfere here
    }

    private MovementState CalculateMovementState(float speed, bool isRunning)
    {
        if (speed < stateChangeThreshold)
            return MovementState.Idle;
        
        if (isRunning && speed > 2f)
            return MovementState.Running;
        
        return MovementState.Walking;
    }

    #region Network State Callbacks

    private void OnMovementStateChanged(MovementState previous, MovementState current)
    {
        currentMovementState = current;
        ApplyCurrentNetworkState();
    }

    private void OnActionStateChanged(ActionState previous, ActionState current)
    {
        currentActionState = current;
        
        // Track when action states start for auto-clearing
        if (current != ActionState.None && previous == ActionState.None)
        {
            actionStateStartTime = Time.time;
        }
        
        ApplyCurrentNetworkState();
    }

    #endregion

    #region Animation Application

    private void ApplyCurrentNetworkState()
    {
        ApplyAnimationState(networkMovementState.Value, networkActionState.Value);
    }

    private void ApplyAnimationState(MovementState movement, ActionState action)
    {
        if (!animator) return;

        // Set animator parameters
        animator.SetInteger(movementStateParam, (int)movement);
        animator.SetInteger(actionStateParam, (int)action);

        // Debug logging (remove in production)
        if (IsOwner)
        {
            Debug.Log($"Animation State: Movement={movement}, Action={action}");
        }
    }

    #endregion

    #region Public Action Methods

    public void TriggerStopAction()
    {
        if (IsOwner)
        {
            networkActionState.Value = ActionState.None;
        }
        else if (IsServer)
        {
            SetActionStateServerRpc(ActionState.None);
        }
    }

    // One-shot trigger methods that auto-clear
    public void TriggerBiteActionOneShot()
    {
        TriggerBiteAction();
        Invoke(nameof(ClearActionState), 0.1f);
    }

    public void TriggerBreathActionOneShot()
    {
        TriggerBreathingAction();
        Invoke(nameof(ClearActionState), 0.1f);
    }

    public void TriggerAttackActionOneShot()
    {
        TriggerAttackAction();
        Invoke(nameof(ClearActionState), 0.1f);
    }

    public void TriggerRevealingActionOneShot()
    {
        TriggerRevealingAction();
        Invoke(nameof(ClearActionState), 0.1f);
    }

    public void TriggerBiteAction()
    {
        if (IsOwner)
        {
            networkActionState.Value = ActionState.Biting;
        }
        else if (IsServer)
        {
            SetActionStateServerRpc(ActionState.Biting);
        }
    }

    public void TriggerBeingBittenAction()
    {
        if (IsOwner)
        {
            networkActionState.Value = ActionState.BeingBitten;
        }
        else if (IsServer)
        {
            SetActionStateServerRpc(ActionState.BeingBitten);
        }
    }

    public void TriggerAttackAction()
    {
        if (IsOwner)
        {
            networkActionState.Value = ActionState.Attacking;
        }
        else if (IsServer)
        {
            SetActionStateServerRpc(ActionState.Attacking);
        }
    }

    public void TriggerBreathingAction()
    {
        if (IsOwner)
        {
            networkActionState.Value = ActionState.Breathing;
        }
        else if (IsServer)
        {
            SetActionStateServerRpc(ActionState.Breathing);
        }
    }

    public void TriggerRevealingAction()
    {
        if (IsOwner)
        {
            networkActionState.Value = ActionState.Revealing;
        }
        else if (IsServer)
        {
            SetActionStateServerRpc(ActionState.Revealing);
        }
    }

    public void TriggerInteractingAction()
    {
        if (IsOwner)
        {
            networkActionState.Value = ActionState.Interacting;
        }
        else if (IsServer)
        {
            SetActionStateServerRpc(ActionState.Interacting);
        }
    }

    public void TriggerClimbingAction()
    {
        if (IsOwner)
        {
            networkActionState.Value = ActionState.Climbing;
        }
        else if (IsServer)
        {
            SetActionStateServerRpc(ActionState.Climbing);
        }
    }

    public void ClearActionState()
    {
        if (IsOwner)
        {
            networkActionState.Value = ActionState.None;
        }
        else if (IsServer)
        {
            SetActionStateServerRpc(ActionState.None);
        }
    }

    // Movement state methods for jumping states
    public void TriggerJumpingState()
    {
        SetMovementStateOverride(MovementState.Jumping);
    }

    public void TriggerFloatingState()
    {
        SetMovementStateOverride(MovementState.Floating);
    }

    public void TriggerFallingState()
    {
        SetMovementStateOverride(MovementState.Landing);
    }

    // Helper method to set movement state overrides
    private void SetMovementStateOverride(MovementState state)
    {
        movementStateOverride = state;
        hasMovementStateOverride = true;
        movementStateOverrideTime = Time.time;
        
        // Apply the override
        if (IsOwner)
        {
            // Owner updates the network variable
            currentMovementState = state;
            networkMovementState.Value = currentMovementState;
        }
        else
        {
            // Non-owners just apply locally for immediate feedback
            currentMovementState = state;
            ApplyAnimationState(state, networkActionState.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetActionStateServerRpc(ActionState newState)
    {
        // Find the owner and update their state
        if (TryGetComponent<NetworkObject>(out var netObj))
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(netObj.OwnerClientId, out var client))
            {
                if (client.PlayerObject == netObj)
                {
                    networkActionState.Value = newState;
                }
            }
        }
    }

    #endregion
}
