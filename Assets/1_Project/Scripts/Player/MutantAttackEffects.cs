using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class MutantAttackEffects : NetworkBehaviour
{
    private InputManager_Singleton _input;
    public VisualEffect visualEffect;
    public Animator animator;

    void Start()
    {
        if (IsOwner)
        {
            _input = InputManager_Singleton.Instance;

            BindButton();
        }
    }

    void BindButton()
    {
        _input._playerInput.Player.Interact4.performed += Handle_Interact4Performed;
    }

    private void Handle_Interact4Performed(InputAction.CallbackContext context)
    {
        if (visualEffect != null)
        {
            visualEffect.SendEvent("Attack");
        }
        if (animator != null)
        {
            animator.SetTrigger("ClawAttack");
        }
    }
}
