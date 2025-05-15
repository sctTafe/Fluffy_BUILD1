using UnityEngine;

public class UprightController : MonoBehaviour
{
    // How strongly the body corrects its tilt.
    public float uprightTorque = 10f;
    // Damping to reduce oscillation.
    public float damping = 0.5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Calculate the current "up" and the desired "up" vector.
        Vector3 currentUp = transform.up;
        Vector3 desiredUp = Vector3.up;

        // Determine the axis and angle required to rotate from current to desired up.
        Vector3 torqueAxis = Vector3.Cross(currentUp, desiredUp);
        float angleDifference = Vector3.Angle(currentUp, desiredUp);

        // Calculate corrective torque based on the angle difference.
        Vector3 correctiveTorque = torqueAxis * angleDifference * uprightTorque;
        // Apply damping to smooth out the rotation.
        correctiveTorque -= rb.angularVelocity * damping;

        rb.AddTorque(correctiveTorque, ForceMode.Acceleration);
    }
}
