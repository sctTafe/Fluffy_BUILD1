using UnityEngine;
using StarterAssets;

public class PlayerDustSteps : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustEmitter;
    [Header("Effect Settings")]
    [SerializeField] private int walkDustCount = 2;
    [SerializeField] private int jumpSmokeCount = 10;
    [SerializeField] private int landingSmokeCount = 15;
    
    [Header("Randomness Settings")]
    [SerializeField] private float positionRandomness = 0.3f;
    [SerializeField] private float angleRandomness = 15f; // degrees
    [SerializeField] private float radiusRandomness = 0.2f;

    public void EmitFootstepDust()
    {
        if (dustEmitter == null) return;
        
        // Check if character is grounded before emitting dust
        if (!IsGrounded()) return;
        
        dustEmitter.Emit(walkDustCount);
    }
    
    public void EmitJumpCloud()
    {
        if (dustEmitter == null) return;

        for (int i = 0; i < jumpSmokeCount; i++)
        {
            // Add randomness to the angle
            float baseAngle = (i / (float)jumpSmokeCount) * 360f;
            float randomAngle = baseAngle + Random.Range(-angleRandomness, angleRandomness);
            float angle = randomAngle * Mathf.Deg2Rad;
            
            // Add randomness to the radius
            float baseRadius = 0.5f;
            float randomRadius = baseRadius + Random.Range(-radiusRandomness, radiusRandomness);
            
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            
            // Add random position offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-positionRandomness, positionRandomness),
                Random.Range(-positionRandomness * 0.5f, positionRandomness * 0.5f), // Less Y variation
                Random.Range(-positionRandomness, positionRandomness)
            );

            var emitParams = new ParticleSystem.EmitParams();
            emitParams.position = transform.position + direction * randomRadius + randomOffset;
            emitParams.velocity = direction * 1.25f; // Outward velocity

            dustEmitter.Emit(emitParams, 1);
        }
    }

    public void EmitLandingCloud()
    {
        if (dustEmitter == null) return;
        
        for (int i = 0; i < landingSmokeCount; i++)
        {
            // Add randomness to the angle
            float baseAngle = (i / (float)landingSmokeCount) * 360f;
            float randomAngle = baseAngle + Random.Range(-angleRandomness, angleRandomness);
            float angle = randomAngle * Mathf.Deg2Rad;
            
            // Add randomness to the radius
            float baseRadius = 0.5f;
            float randomRadius = baseRadius + Random.Range(-radiusRandomness, radiusRandomness);
            
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            
            // Add random position offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-positionRandomness, positionRandomness),
                Random.Range(-positionRandomness * 0.3f, positionRandomness * 0.3f), // Minimal Y variation for ground effect
                Random.Range(-positionRandomness, positionRandomness)
            );

            var emitParams = new ParticleSystem.EmitParams();
            emitParams.position = transform.position + direction * randomRadius + randomOffset;
            emitParams.velocity = direction * 2f; // Outward velocity

            dustEmitter.Emit(emitParams, 1);
        }
    }

    /// <summary>
    /// Checks if the character is grounded by looking for movement controllers on this GameObject
    /// </summary>
    private bool IsGrounded()
    {
        // Try to find a ThirdPersonController (Standard Unity Starter Assets)
        var thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
        if (thirdPersonController != null)
            return thirdPersonController.Grounded;

        // Try to find the Netcode version
        var thirdPersonControllerNetcode = GetComponent<StarterAssets.ThirdPersonController_Netcode>();
        if (thirdPersonControllerNetcode != null)
            return thirdPersonControllerNetcode.Grounded;

        // Try to find Scott's backup controller
        var scottsBackupController = GetComponent<ScottsBackup_ThirdPersonController>();
        if (scottsBackupController != null)
            return scottsBackupController.Grounded;

        // Try to find AnimalCharacter controller
        var animalCharacter = GetComponent<AnimalCharacter>();
        if (animalCharacter != null)
            return animalCharacter.IsGrounded;

        // If no movement controller found, default to true (fail-safe)
        Debug.LogWarning($"No movement controller found on {gameObject.name} for grounded check. Defaulting to grounded = true.");
        return true;
    }
}
