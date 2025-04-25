using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DestroyBush : NetworkBehaviour
{


    private InputManager_Singleton _input;
    public InputActionV2 _inputActions;

    public BoxCollider collider;
    public string targetTag = "hide_trigger";
    //public LayerMask targetLayer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsOwner)
        {
            _input = InputManager_Singleton.Instance;

            this._inputActions = _input._inputActions;


            BindButton();
        }

    }

    void BindButton()
    {
        _inputActions.Player.Interact4.performed += Handle_Interact4Performed;
    }

    private void Handle_Interact4Performed(InputAction.CallbackContext context)
    {
        //Debug.Log("Interact 4 ('F') Pressed");

        if (!IsABushInsideCollider(out GameObject biteTarget))
        {
            Debug.Log("DestroyBush: No bush found!");
            return;
        }
    }




    public bool IsABushInsideCollider(out GameObject? Target)
    {
        Target = null;

        if (collider == null || !collider.isTrigger)
        {
            Debug.LogWarning("BoxCollider is null or not set as trigger!");
            return false;
        }

        // Calculate world-space bounds of the box
        Vector3 center = collider.transform.TransformPoint(collider.center);
        Vector3 halfExtents = Vector3.Scale(collider.size * 0.5f, collider.transform.lossyScale);
        Quaternion orientation = collider.transform.rotation;

        // Check all overlapping colliders
        Collider[] hits = Physics.OverlapBox(center, halfExtents, orientation);

        foreach (Collider hit in hits)
        {
            if (hit == collider) continue; // Skip self

            if (hit.CompareTag(targetTag)) //||  (targetLayer.value == hit.gameObject.layer)
            {
                Target = hit.gameObject;
                return true;
            }
        }

        return false;
    }
}
