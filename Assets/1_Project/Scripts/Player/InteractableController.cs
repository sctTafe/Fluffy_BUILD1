using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableController : NetworkBehaviour
{
    // Interaction Variables
    [Header("Interaction Variables")]
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] private Transform _interactableCheckPoint;
    private float _interationCheckRadius = 1.5f;
    private bool isCurrentlyInteracting;
    private bool isCurrentlyInteracting2;

    // Core
    private IHolder_Player _playerItemHolder;
    private InputManager_Singleton _input;
    public InputActionV2 _inputActions;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsOwner)
        {
            _input = InputManager_Singleton.Instance;
            _playerItemHolder = GetComponent<IHolder_Player>();
            this._inputActions = _input._inputActions;


            BindButton();
        }
    }

    void BindButton()
    {
        _inputActions.Player.Interact1.performed += Handle_Interact1Performed;
        _inputActions.Player.Interact2.performed += Handle_Interact2Performed;
        _inputActions.Player.Interact3.performed += Handle_Interact3Performed;
        _inputActions.Player.Interact4.performed += Handle_Interact4Performed;
    }

    private void Handle_Interact1Performed(InputAction.CallbackContext context)
    {
        Debug.Log("Interact 2 ('E') Pressed");

        Debug.Log($" Player {this.gameObject.name} is trying to Interact 'e'");
        CheckForInteractables();
    }

    private void Handle_Interact2Performed(InputAction.CallbackContext context)
    {
        Debug.Log("Interact 1 ('R') Pressed");

        // TriggerHoldableItemTriggerA(); //TODO: need to add null check for inventory
    }

    private void Handle_Interact3Performed(InputAction.CallbackContext context)
    {
        Debug.Log("Interact 3 ('Q') Pressed");
        //_playerItemHolder.fn_CycleItemSlots(); //TODO: need to add null check for inventory

    }

    private void Handle_Interact4Performed(InputAction.CallbackContext context)
    {
        Debug.Log("Interact 4 ('F') Pressed");
    }




    private void CheckForInteractables()
    {
        IInteractable closestInteractable = null;
        float distanceToInteractable = float.MaxValue;

        Collider[] hits = Physics.OverlapSphere(_interactableCheckPoint.position, _interationCheckRadius, interactableLayer);

        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable != null)
            {

                float distance = Vector3.Distance(_interactableCheckPoint.position, hit.transform.position);

                // Check if this interactable is closer than the previous one found
                if (distance < distanceToInteractable)
                {
                    closestInteractable = interactable;
                    distanceToInteractable = distance;
                }
            }
        }

        // If a closest interactable object was found, interact with it
        if (closestInteractable != null)
        {
            Debug.Log($"Found closest interactable object: {closestInteractable}");
            closestInteractable.TryInteract(this.NetworkObject);
        }
    }

    private void TriggerHoldableItemTriggerA()
    {
        _playerItemHolder.fn_DropSlot0Item();
    }
}
