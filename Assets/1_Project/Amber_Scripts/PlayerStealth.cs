using UnityEngine;
using Unity.Netcode;
using TMPro;
using FMODUnity;
using UnityEngine.Events;

public class PlayerStealth : NetworkBehaviour
{
    [SerializeField] private GameObject geometry;
    private Renderer[] renderers;
    private Material[][] materialInstances; // Store material instances

    // Local owner-only bush tracking
    private bool in_bush = false;
    private float time_in_bush = 0f;
    private float force_reveal = 0f; // countdown while force revealed (owner only)

    private TMP_Text stealth_prompt;
    private bool played_sound = false;

    // Unity Event & HUD popup integration (migrated from backup script)
    [Header("Stealth Events")] public UnityEvent OnPlayerEnterBush_Local; // invoked when local player transitions to hiding
    private HUD_PopUpMessages_Singelton hudPopup; // owner-only reference

    // Dissolve shader control
    [Header("Dissolve Settings")] 
    [SerializeField] private string dissolvePropertyName = "_DitherAlpha";
    [SerializeField] private float dissolveSpeed = 2f; // Units (0-1) per second
    [SerializeField] private float hiddenDissolveValue = 0.0f;
    [SerializeField] private float visibleDissolveValue = 1.0f;
    [Tooltip("Clamp the per-frame dt so a single large frame hitch can't instantly jump the dissolve value")] [SerializeField]
    private float maxDissolveDeltaTime = 0.05f; // 50 ms cap
    [Tooltip("Enable verbose logging for dissolve debugging")] [SerializeField]
    private bool debugDissolve = false;

    private float targetDissolveValue = 1.0f;
    private float currentDissolveValue = 1.0f;

    public EventReference hide_sound;

    // Networked stealth state (minimal bandwidth)
    private enum StealthNetState : byte { Visible, Hiding, Hidden, Revealing }

    private NetworkVariable<StealthNetState> netState = new NetworkVariable<StealthNetState>(
        StealthNetState.Visible,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private StealthNetState lastAppliedState = StealthNetState.Visible; // for local change detection (non-owner)

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Subscribe to state updates for non-owners
            netState.OnValueChanged += OnNetStateChanged;
        }
        else
        {
            // Ensure initial push
            netState.Value = StealthNetState.Visible;
        }
    }

    void Start()
    {
        renderers = geometry.GetComponentsInChildren<Renderer>();
        if (debugDissolve) Debug.Log($"[Stealth] Found {renderers.Length} renderers");
        InitializeMaterialInstances();
        UpdateDissolveShader(currentDissolveValue);

        if (IsOwner)
        {
            GameObject stealthObject = GameObject.FindWithTag("stealth_prompt");
            if (stealthObject)
                stealth_prompt = stealthObject.GetComponent<TMP_Text>();
            hudPopup = HUD_PopUpMessages_Singelton.Instance; // may be null if not yet loaded
        }
    }

    void InitializeMaterialInstances()
    {
        materialInstances = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Material[] originalMaterials = renderers[i].materials; // creates instances already
            Material[] instanceMaterials = new Material[originalMaterials.Length];
            for (int j = 0; j < originalMaterials.Length; j++)
            {
                if (originalMaterials[j] == null) continue;
                instanceMaterials[j] = new Material(originalMaterials[j]);
                if (debugDissolve && instanceMaterials[j].HasProperty(dissolvePropertyName))
                {
                    Debug.Log($"[Stealth] Mat {originalMaterials[j].name} has {dissolvePropertyName}");
                }
            }
            renderers[i].materials = instanceMaterials; // assign instanced copies
            materialInstances[i] = instanceMaterials;
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            OwnerDriveStateMachine();
        }
        else
        {
            // Non-owner: ensure target matches network state
            ApplyNetState(netState.Value);
        }

        // Smooth dissolve towards target
        if (!Mathf.Approximately(currentDissolveValue, targetDissolveValue))
        {
            float dt = Time.deltaTime;
            if (dt > maxDissolveDeltaTime) dt = maxDissolveDeltaTime;
            currentDissolveValue = Mathf.MoveTowards(currentDissolveValue, targetDissolveValue, dissolveSpeed * dt);
            UpdateDissolveShader(currentDissolveValue);
        }

        // Owner updates terminal states when fades complete
        if (IsOwner)
        {
            if (netState.Value == StealthNetState.Hiding && Mathf.Approximately(currentDissolveValue, hiddenDissolveValue))
                netState.Value = StealthNetState.Hidden;
            else if (netState.Value == StealthNetState.Revealing && Mathf.Approximately(currentDissolveValue, visibleDissolveValue))
                netState.Value = StealthNetState.Visible;
        }
    }

    private void OwnerDriveStateMachine()
    {
        // Forced reveal countdown
        force_reveal -= Time.deltaTime;

        // Bush timer only if not force revealed
        if (in_bush && force_reveal <= 0f)
        {
            time_in_bush += Time.deltaTime;
            if (time_in_bush > 0.8f && netState.Value == StealthNetState.Visible)
            {
                // Start hiding
                netState.Value = StealthNetState.Hiding;
                SetTargetHidden();
                ShowOwnerPrompt("[ Hidden! ]");
                InvokeOwnerEnterBushEvent();
                if (!played_sound)
                {
                    play_hide_sound();
                    played_sound = true;
                }
            }
        }
        else
        {
            played_sound = false;
        }

        // Force reveal takes priority
        if (force_reveal > 0f)
        {
            if (netState.Value != StealthNetState.Revealing && netState.Value != StealthNetState.Visible)
            {
                netState.Value = StealthNetState.Revealing;
                SetTargetVisible();
                ShowOwnerPrompt("[ Revealed! ]");
            }
        }

        // If exited bush and currently hidden or hiding, begin reveal (unless force reveal already doing it)
        if (!in_bush && force_reveal <= 0f)
        {
            if (netState.Value == StealthNetState.Hidden || netState.Value == StealthNetState.Hiding)
            {
                netState.Value = StealthNetState.Revealing;
                SetTargetVisible();
                ClearOwnerPrompt();
                ShowOwnerPopupNeutral();
            }
        }

        // Ensure local target matches current state (handles immediate transitions)
        ApplyNetState(netState.Value);
    }

    private void OnNetStateChanged(StealthNetState previous, StealthNetState current)
    {
        ApplyNetState(current);
    }

    private void ApplyNetState(StealthNetState state)
    {
        if (state == lastAppliedState) return;
        switch (state)
        {
            case StealthNetState.Visible:
                SetTargetVisible();
                break;
            case StealthNetState.Hiding:
            case StealthNetState.Hidden: // Hidden keeps target at hidden value
                SetTargetHidden();
                break;
            case StealthNetState.Revealing:
                SetTargetVisible();
                break;
        }
        lastAppliedState = state;
    }

    private void SetTargetHidden() => targetDissolveValue = hiddenDissolveValue;
    private void SetTargetVisible() => targetDissolveValue = visibleDissolveValue;

    private void ShowOwnerPrompt(string txt)
    {
        if (IsOwner && stealth_prompt) stealth_prompt.text = txt;
        if (IsOwner && hudPopup != null)
        {
            hudPopup.fn_PopupMessage(txt, HUD_PopUpMessages_Singelton.PopupStyle.PopAndFade);
        }
    }
    private void ClearOwnerPrompt()
    {
        if (IsOwner && stealth_prompt) stealth_prompt.text = string.Empty;
    }

    private void ShowOwnerPopupNeutral()
    {
        if (IsOwner && hudPopup != null)
        {
            hudPopup.fn_PopupMessage("[ - - - ]", HUD_PopUpMessages_Singelton.PopupStyle.PopAndFade);
        }
    }

    private void InvokeOwnerEnterBushEvent()
    {
        if (IsOwner && OnPlayerEnterBush_Local != null)
        {
            OnPlayerEnterBush_Local.Invoke();
        }
    }

    void UpdateDissolveShader(float dissolveValue)
    {
        for (int i = 0; i < materialInstances.Length; i++)
        {
            if (materialInstances[i] == null) continue;
            for (int j = 0; j < materialInstances[i].Length; j++)
            {
                Material mat = materialInstances[i][j];
                if (mat != null && mat.HasProperty(dissolvePropertyName))
                {
                    mat.SetFloat(dissolvePropertyName, dissolveValue);
                }
            }
        }
    }

    void play_hide_sound()
    {
        if (hide_sound.IsNull) return;
        RuntimeManager.PlayOneShot(hide_sound, transform.position);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return; // only owner drives state
        if (other.CompareTag("hide_trigger"))
        {
            in_bush = true;
            time_in_bush = 0f;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;
        if (other.CompareTag("hide_trigger"))
        {
            in_bush = false;
            time_in_bush = 0f;
            // If currently hiding or hidden, start reveal next Update()
        }
    }

    // Forces the player to reveal for 10 seconds (scan attack)
    public void force_unhide()
    {
        if (IsOwner)
        {
            force_reveal = 10f;
            netState.Value = StealthNetState.Revealing;
            SetTargetVisible();
            ShowOwnerPrompt("[ Revealed! ]");
        }
        else
        {
            // Request owner to reveal via server
            ForceUnhideServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ForceUnhideServerRpc(ServerRpcParams rpcParams = default)
    {
        // Route to owner only (owner will set netState)
        ForceUnhideClientRpc(OwnerClientId);
    }

    [ClientRpc]
    private void ForceUnhideClientRpc(ulong ownerId, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner && NetworkManager.LocalClientId == ownerId)
        {
            force_reveal = 10f;
            netState.Value = StealthNetState.Revealing;
            SetTargetVisible();
            ShowOwnerPrompt("[ Revealed! ]");
        }
    }

    [ContextMenu("Test Dissolve (Hide)")]
    public void TestDissolve()
    {
        if (IsOwner)
        {
            netState.Value = StealthNetState.Hiding;
            SetTargetHidden();
            currentDissolveValue = hiddenDissolveValue;
            UpdateDissolveShader(currentDissolveValue);
            ShowOwnerPrompt("[ Hidden! ]");
            InvokeOwnerEnterBushEvent();
        }
    }

    [ContextMenu("Test Reveal")]
    public void TestReveal()
    {
        if (IsOwner)
        {
            netState.Value = StealthNetState.Revealing;
            SetTargetVisible();
            currentDissolveValue = visibleDissolveValue;
            UpdateDissolveShader(currentDissolveValue);
            ShowOwnerPrompt("[ Revealed! ]");
        }
    }

    void OnDestroy()
    {
        if (netState != null)
        {
            netState.OnValueChanged -= OnNetStateChanged;
        }
        if (materialInstances != null)
        {
            for (int i = 0; i < materialInstances.Length; i++)
            {
                if (materialInstances[i] == null) continue;
                for (int j = 0; j < materialInstances[i].Length; j++)
                {
                    if (materialInstances[i][j] != null)
                    {
                        Destroy(materialInstances[i][j]);
                    }
                }
            }
        }
    }
}
