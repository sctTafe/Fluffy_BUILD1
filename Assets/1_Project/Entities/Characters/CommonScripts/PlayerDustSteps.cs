using UnityEngine;

public class PlayerDustSteps : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustEmitter;
    
    public void EmitDust()
    {
        if (dustEmitter == null) return;
        //dustEmitter.Play(); // Plays burst, if set up in Emission module
        dustEmitter.Emit(2);
    }
}
