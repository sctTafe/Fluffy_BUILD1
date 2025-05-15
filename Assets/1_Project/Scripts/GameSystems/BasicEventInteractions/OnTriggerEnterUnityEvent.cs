using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEnterUnityEvent : MonoBehaviour
{
    public UnityEvent onTagTypeEnterCollider;
    public string tagToDetect = "Mutant";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagToDetect))
        {
            onTagTypeEnterCollider?.Invoke();
        }
    }
}
