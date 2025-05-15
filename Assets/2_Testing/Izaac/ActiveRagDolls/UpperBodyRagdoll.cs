using System.Collections.Generic;
using UnityEngine;

public class UpperBodyRagdoll : MonoBehaviour
{
    [Header("Ragdoll Parts")]
    [SerializeField] private Animator animator;
    [SerializeField] private List<Rigidbody> upperBodyRigidbodies;
    [SerializeField] private List<ConfigurableJoint> upperBodyJoints;

    [Header("PD Controller Settings")]
    [SerializeField] private float positionSpring = 500f;
    [SerializeField] private float positionDamper = 50f;

    [Header("Grabbing Settings")]
    [SerializeField] private Transform hand;
    [SerializeField] private float grabRange = 2f;
    [SerializeField] private LayerMask grabbableLayer;

    private bool isRagdollActive = false;
    private Transform grabbedObject;

    private void Start()
    {
        DisableUpperBodyRagdoll();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))  // Toggle ragdoll
        {
            if (isRagdollActive)
                DisableUpperBodyRagdoll();
            else
                EnableUpperBodyRagdoll();
        }

        if (isRagdollActive && Input.GetKeyDown(KeyCode.E)) // Grab object
        {
            TryGrabObject();
        }
        else if (!isRagdollActive && grabbedObject != null) // Release object
        {
            ReleaseObject();
        }
    }

    /// <summary>
    /// Enables ragdoll physics for the upper body and disables animation influence.
    /// </summary>
    public void EnableUpperBodyRagdoll()
    {
        isRagdollActive = true;

        foreach (Rigidbody rb in upperBodyRigidbodies)
        {
            rb.isKinematic = false; // Enable physics
        }

        foreach (ConfigurableJoint joint in upperBodyJoints)
        {
            var drive = joint.slerpDrive;
            drive.positionSpring = positionSpring;
            drive.positionDamper = positionDamper;
            joint.slerpDrive = drive;
        }

        animator.SetLayerWeight(1, 0); // Reduce animation influence
    }

    /// <summary>
    /// Disables ragdoll physics and re-enables animation for upper body.
    /// </summary>
    public void DisableUpperBodyRagdoll()
    {
        isRagdollActive = false;

        foreach (Rigidbody rb in upperBodyRigidbodies)
        {
            rb.isKinematic = true; // Disable physics
        }

        foreach (ConfigurableJoint joint in upperBodyJoints)
        {
            var drive = joint.slerpDrive;
            drive.positionSpring = 0f;
            drive.positionDamper = 0f;
            joint.slerpDrive = drive;
        }

        animator.SetLayerWeight(1, 1); // Restore animation influence
    }

    /// <summary>
    /// Makes the character attempt to grab an object in front of them.
    /// </summary>
    private void TryGrabObject()
    {
        Collider[] hits = Physics.OverlapSphere(hand.position, grabRange, grabbableLayer);
        if (hits.Length > 0)
        {
            grabbedObject = hits[0].transform;
            grabbedObject.SetParent(hand);
            grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    /// <summary>
    /// Releases a grabbed object.
    /// </summary>
    private void ReleaseObject()
    {
        if (grabbedObject)
        {
            grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
            grabbedObject.SetParent(null);
            grabbedObject = null;
        }
    }
}
