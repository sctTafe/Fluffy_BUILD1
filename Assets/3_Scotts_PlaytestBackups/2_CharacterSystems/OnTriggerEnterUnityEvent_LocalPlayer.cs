using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEnterUnityEvent_LocalPlayer : MonoBehaviour
{
    public UnityEvent onTagTypeEnterCollider;
    public UnityEvent onTagTypeExitCollider;
    public string tagToDetect = "Mutant";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagToDetect))
        {
            if (other.TryGetComponent<ScottsBackup_IsLocalPlayerCheck>(out var lp))
            {
                if (lp._isLocalPlayer) 
                {
                    onTagTypeEnterCollider?.Invoke();
                }            
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag(tagToDetect))
        {
            if (other.TryGetComponent<ScottsBackup_IsLocalPlayerCheck>(out var lp))
            {
                if (lp._isLocalPlayer)
                {
                    onTagTypeExitCollider?.Invoke();
                }
            }           
        }
    }
}
