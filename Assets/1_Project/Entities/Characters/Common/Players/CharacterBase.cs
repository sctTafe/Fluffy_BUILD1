using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public abstract class CharacterBase : NetworkBehaviour
{
    protected Rigidbody _rigidbody;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public virtual void OnPossess()
    {
        Debug.Log($"{name} possessed!");
    }

    public virtual void OnUnpossess()
    {
        Debug.Log($"{name} unpossessed!");
    }

    // Define abstract methods for behaviors that must be implemented by derived classes.
    public abstract void HandleMovement(Vector2 input);


    public virtual void Attack()
    {
        return;
    }
    
}