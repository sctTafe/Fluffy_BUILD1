using Unity.Netcode;
using UnityEngine;

public class CharacterAnimator : NetworkBehaviour
{
    private Animator animator;

    private float forwardSpeed;
    private float sidewaysSpeed;

    [SerializeField] private string forwardParam = "ForwardsSpeed";
    [SerializeField] private string sidewaysParam = "SidewaysSpeed";
    [SerializeField] private string jumpStateParam = "JumpState";
    [SerializeField] private string beingBittenParam = "BeingBitten";
    [SerializeField] private string isBittingParam = "IsBitting";


    [Header("Smoothing")]
    [Tooltip("Higher values snap faster; lower values smooth more")] [SerializeField]
    private float smoothing = 10f;

    // smoothed values used to drive animator to avoid jitter at low speeds
    private float _smoothedForward;
    private float _smoothedSideways;
    
    // Network variable to sync bite state - make it public to ensure proper registration
    public NetworkVariable<bool> isBeingBitten = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isBitting = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Use LateUpdate to ensure the animator is updated after all possible changes
    private void LateUpdate()
    {
        if (animator != null && IsSpawned)
        {
            animator.SetBool(beingBittenParam, isBeingBitten.Value);
            animator.SetBool(isBittingParam, isBitting.Value);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isBeingBitten.OnValueChanged += OnBeingBittenStateChanged;
        isBitting.OnValueChanged += OnBittingStateChanged;

        if (animator != null)
        {
            animator.SetBool(beingBittenParam, isBeingBitten.Value);
            animator.SetBool(isBittingParam, isBitting.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        isBeingBitten.OnValueChanged -= OnBeingBittenStateChanged;
        isBitting.OnValueChanged -= OnBittingStateChanged;
        base.OnNetworkDespawn();
    }


    private void OnBeingBittenStateChanged(bool previous, bool current)
    {
        if (animator != null)
        {
            animator.SetBool(beingBittenParam, current);
        }
    }

    private void OnBittingStateChanged(bool previous, bool current)
    {
        if (animator != null)
        {
            animator.SetBool(isBittingParam, current);
        }
    }

    /// <summary>
    /// Updates animator parameters using movement data.
    /// For the owner, calculate local velocity; for remote clients, use networked values.
    /// </summary>
    public void UpdateAnimatorLocomotion(Vector3 velocity, Transform characterTransform, bool isOwner, float networkSideways, float networkForward)
    {
        if (!animator) return;

        if (isOwner)
        {
            Vector3 localVelocity = characterTransform.InverseTransformDirection(velocity);
            forwardSpeed = localVelocity.z;
            sidewaysSpeed = localVelocity.x;
        }
        else
        {
            forwardSpeed = networkForward;
            sidewaysSpeed = networkSideways;
        }

        // smooth small fluctuations to avoid animator stutter at low speeds
        _smoothedForward = Mathf.Lerp(_smoothedForward, forwardSpeed, Time.deltaTime * smoothing);
        _smoothedSideways = Mathf.Lerp(_smoothedSideways, sidewaysSpeed, Time.deltaTime * smoothing);

        animator.SetFloat(forwardParam, _smoothedForward);
        animator.SetFloat(sidewaysParam, _smoothedSideways);
    }

    /// <summary>
    /// Updates the jump state parameter in the animator.
    /// 0 = Normal locomotion, 1 = Jump start, 2 = Float (loops), 3 = Land
    /// </summary>
    public void UpdateJumpState(int jumpState)
    {
        if (!animator) return;
        animator.SetInteger(jumpStateParam, jumpState);
    }

    // ---------------------------------------------------------------
    /// Fluffy being bitten animations
    public void TriggerBeingBittenAnimation()
    {
        if (IsServer)
        {
            isBeingBitten.Value = true;
        }
        else
        {
            SetBittenStateServerRpc(true);
        }
    }

    public void TriggerStopBeingBittenAnimation()
    {
        if (IsServer)
        {
            isBeingBitten.Value = false;
        }
        else
        {
            SetBittenStateServerRpc(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBittenStateServerRpc(bool state)
    {
        isBeingBitten.Value = state;
    }

    //---------------------------------------------------------------
    /// Mutant bitting animations
    public void TriggerBittingAnimation()
    {
        if (IsServer)
        {
            isBitting.Value = true;
        }
        else
        {
            SetBiteStateServerRpc(true);
        }
    }

    public void TriggerStopBittingAnimation()
    {
        if (IsServer)
        {
            isBitting.Value = false;
        }
        else
        {
            SetBiteStateServerRpc(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBiteStateServerRpc(bool state)
    {
        isBitting.Value = state;
    }
    //---------------------------------------------------------------
}
