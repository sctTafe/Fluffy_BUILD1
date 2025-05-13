using UnityEngine;

public class ScottsBackup_MapObjectStealthTrigger : MonoBehaviour
{
    [SerializeField] private string _tag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_tag))
        {
            other.GetComponent<ScottsBackup_PlayerStealthMng>().fn_SetInBush();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_tag))
        {
            other.GetComponent<ScottsBackup_PlayerStealthMng>().fn_SetLeavingBush();
        }
    }
}
