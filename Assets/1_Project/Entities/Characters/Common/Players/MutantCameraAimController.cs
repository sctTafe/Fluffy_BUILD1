using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Controls the player's aiming core by reading look/aim input 
/// from the InputManager_Singleton and converting it into yaw and pitch rotations.
/// The aiming core decouples camera rotation from player rotation so that the camera 
/// can orbit independently while still providing a basis for player direction and shooting.
/// </summary>
public class MutantCameraAimController : MonoBehaviour
{
    public enum CouplingMode { Coupled, CoupledWhenMoving, Decoupled }

    [Tooltip("How the player's rotation is coupled to the camera's rotation.")]
    public CouplingMode PlayerRotation = CouplingMode.Coupled;

    [Tooltip("How fast the player rotates to face the camera direction when the player starts moving. " +
             "Only used when Player Rotation is Coupled When Moving.")]
    public float RotationDamping = 0.2f;

    [Tooltip("Reference to the player's character/controller.")]
    public MutantCharacter m_Controller;

    private Transform m_ControllerTransform; // Cached for efficiency.
    private Quaternion m_DesiredWorldRotation;

    // Internal yaw (horizontal) and pitch (vertical) angles in degrees.
    private float yaw;
    private float pitch;

    private void Awake()
    {
        // Initialize our aim values based on the current local rotation.
        Vector3 initialEuler = transform.localRotation.eulerAngles;
        yaw = initialEuler.y;
        pitch = initialEuler.x;
    }

    private void OnEnable()
    {
        // If not already assigned, try to find the character controller on a parent.
        if (m_Controller == null)
        {
            m_Controller = GetComponentInParent<MutantCharacter>();
            if (m_Controller == null)
            {
                Debug.LogError("AnimalCharacter not found on parent object");
                return;
            }
        }
        m_ControllerTransform = m_Controller.transform;

        // Subscribe to pre- and post-update events on the character.
        m_Controller.PreUpdate -= UpdatePlayerRotation;
        m_Controller.PreUpdate += UpdatePlayerRotation;
        m_Controller.PostUpdate -= PostUpdate;
        m_Controller.PostUpdate += PostUpdate;
    }

    private void OnDisable()
    {
        if (m_Controller != null)
        {
            m_Controller.PreUpdate -= UpdatePlayerRotation;
            m_Controller.PostUpdate -= PostUpdate;
        }
        m_ControllerTransform = null;
    }

    /// <summary>
    /// Recenters the player by rotating the parent (character) towards the camera’s direction.
    /// Uses a damping value to smooth the adjustment.
    /// </summary>
    /// <param name="damping">Time-based damping value (0 for immediate alignment).</param>
    public void RecenterPlayer(float damping = 0)
    {
        if (m_ControllerTransform == null)
            return;

        // Here we treat our yaw as the horizontal offset.
        float currentYaw = yaw;

        // Apply damping—assuming Damper.Damp is a utility function you have.
        float dampedDelta = Damper.Damp(currentYaw, damping, Time.deltaTime);

        // Rotate the player (the parent) towards the camera’s direction.
        m_ControllerTransform.rotation = Quaternion.AngleAxis(dampedDelta, m_ControllerTransform.up) * m_ControllerTransform.rotation;

        // Adjust our own yaw by subtracting the damped delta.
        yaw -= dampedDelta;

        // Update our local rotation.
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
    }

    /// <summary>
    /// Sets the camera's look direction based on a worldspace direction without altering player rotation.
    /// </summary>
    /// <param name="worldspaceDirection">Direction to look at, in worldspace.</param>
    public void SetLookDirection(Vector3 worldspaceDirection)
    {
        if (m_ControllerTransform == null)
            return;

        Quaternion relativeRot = Quaternion.Inverse(m_ControllerTransform.rotation) *
                                 Quaternion.LookRotation(worldspaceDirection, m_ControllerTransform.up);
        Vector3 euler = relativeRot.eulerAngles;
        yaw = Mathf.Clamp(euler.y, -180f, 180f);
        pitch = Mathf.Clamp(NormalizeAngle(euler.x), -90f, 90f);
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
    }

    /// <summary>
    /// Called by the character controller before updating its own rotation.
    /// Reads the look input from the InputManager_Singleton and adjusts the aim accordingly.
    /// </summary>
    void UpdatePlayerRotation()
    {
        // Process input only for the local (owning) player.
        //if (!m_Controller.NetworkObject.IsOwner)
        //    return;

        // Retrieve current aim input every frame.
        Vector2 aim = InputManager_Singleton.Instance?.look ?? Vector2.zero;

        // Update our internal yaw and pitch.
        yaw += aim.x;
        pitch -= aim.y;
        // Clamp the pitch to avoid extreme vertical angles.
        pitch = Mathf.Clamp(pitch, -70f, 70f);

        // Apply the rotation.
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        m_DesiredWorldRotation = transform.rotation;

        // Handle the coupling between camera and player rotation.
    //    switch (PlayerRotation)
    //    {
    //        case CouplingMode.Coupled:
    //            //m_Controller.SetStrafeMode(true);
    //            RecenterPlayer();
    //            break;
    //
    //        case CouplingMode.CoupledWhenMoving:
    //            //m_Controller.SetStrafeMode(true);
    //            //if (m_Controller.IsMoving)
    //                RecenterPlayer(RotationDamping);
    //            break;
    //
    //        case CouplingMode.Decoupled:
    //            //m_Controller.SetStrafeMode(false);
    //            break;
    //    }
    }

    /// <summary>
    /// Called by the character controller after it has updated its rotation.
    /// Adjusts the aim controller to preserve its desired world rotation.
    /// </summary>
    /// <param name="vel">Current velocity (unused here).</param>
    /// <param name="speed">Current speed (unused here).</param>
    void PostUpdate(Vector3 vel, float speed)
    {
        // Maintain the calculated world rotation.
        transform.rotation = m_DesiredWorldRotation;

        // Calculate the difference between the character's rotation and the desired aim rotation.
        Quaternion deltaRot = Quaternion.Inverse(m_ControllerTransform.rotation) * m_DesiredWorldRotation;
        Vector3 deltaEuler = deltaRot.eulerAngles;

        // Normalize angles and update internal variables.
        pitch = NormalizeAngle(deltaEuler.x);
        yaw = NormalizeAngle(deltaEuler.y);
    }

    /// <summary>
    /// Normalizes an angle to the range -180...180.
    /// </summary>
    float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;
        while (angle < -180f)
            angle += 360f;
        return angle;
    }
}
