using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : NetworkBehaviour
{
    public static PlayerInteractor LocalInstance { get; private set; }
    [SerializeField] private InteractionPromptUI interactPromptUI;
    private PlayerInput playerInput;
    private InteractableObject currentInteractable;

    private void Awake()
    {
        if (LocalInstance == null) LocalInstance = this;
        else Destroy(gameObject);

        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        foreach (ActionKey actionKey in System.Enum.GetValues(typeof(ActionKey)))
        {
            var action = playerInput.actions.FindAction(actionKey.ToString(), throwIfNotFound: false);
            if (action != null)
            {
                action.performed += ctx => HandleInteraction(actionKey);
            }
        }
    }

    private void OnDisable()
    {
        foreach (ActionKey actionKey in System.Enum.GetValues(typeof(ActionKey)))
        {
            var action = playerInput.actions.FindAction(actionKey.ToString(), throwIfNotFound: false);
            if (action != null)
            {
                action.performed -= ctx => HandleInteraction(actionKey);
            }
        }
    }

    private void Update()
    {
        if (currentInteractable == null)
        {
            interactPromptUI.HidePrompt();
            return;
        }

        List<string> options = currentInteractable.GetAssignedActions()
            .Select(opt => $"{opt.actionName} [{opt.actionKey}]")
            .ToList();

        interactPromptUI.ShowPrompt(options);
    }

    private void HandleInteraction(ActionKey actionKey)
    {
        if (currentInteractable == null) return;
        currentInteractable.Interact(actionKey, this);
    }

    public void RegisterInteractable(InteractableObject interactable)
    {
        currentInteractable = interactable;
        Debug.Log($"[Client] Registered interactable: {interactable.gameObject.name}");

        List<string> options = currentInteractable.GetAssignedActions()
            .Select(opt => $"{opt.actionName} [{opt.actionKey}]")
            .ToList();
        interactPromptUI.ShowPrompt(options);
    }

    public void UnregisterInteractable(InteractableObject interactable)
    {
        if (currentInteractable == interactable)
        {
            Debug.Log($"[Client] Unregistered interactable: {interactable.gameObject.name}");
            currentInteractable = null;
            interactPromptUI.HidePrompt();
        }
    }
}
