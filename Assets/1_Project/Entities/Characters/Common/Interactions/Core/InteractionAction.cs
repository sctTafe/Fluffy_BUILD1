using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public enum ActionKey 
{
    // These need to be named the same as the key names defined in your player input component, (on the player interator)
    Interact1,
    Interact2,
    Interact3,
    Interact4
}


public abstract class InteractionAction : NetworkBehaviour
{
    [Header("Assign an Action Key")]
    public ActionKey actionKey;  // Dropdown in Inspector

    // Using a fixed string type that supports serialization.
    private NetworkVariable<FixedString128Bytes> prompt = new NetworkVariable<FixedString128Bytes>();

    public abstract void Execute(PlayerInteractor player);

    public virtual void SetPrompt(string _prompt)
    {
        prompt.Value = new FixedString128Bytes(_prompt);
    }

    public virtual string GetActionName()
    {
        return prompt.Value.ToString();
    }
}
