using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class Toggle_Action : InteractionAction
{
    private NetworkVariable<bool> toggleState = new NetworkVariable<bool>();
    public GameObject objectToToggle;

    public string promptWhenOn = "Turn Off";
    public string promptWhenOff = "Turn On";

    private void Awake()
    {
        toggleState.Value = objectToToggle.activeSelf;
        base.SetPrompt(objectToToggle.activeSelf ? promptWhenOn : promptWhenOff);
        toggleState.OnValueChanged += OnToggleStateChanged;
    }

    private void OnDestroy()
    {
        toggleState.OnValueChanged -= OnToggleStateChanged;
    }

    private void OnToggleStateChanged(bool oldValue, bool newValue)
    {
        objectToToggle.SetActive(newValue);
    }

    public override void Execute(PlayerInteractor player)
    {
        if (IsServer)
        {
            Toggle();
        }
        else
        {
            ExecuteToggleServerRpc();
        }
    }

    private void Toggle()
    {
        toggleState.Value = !toggleState.Value;
        base.SetPrompt(objectToToggle.activeSelf ? promptWhenOn : promptWhenOff);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ExecuteToggleServerRpc(ServerRpcParams rpcParams = default)
    {
        Toggle();
    }

}
