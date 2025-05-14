
using System.Collections.Generic;
using UnityEngine;

public class ScottsBackup_MapObjectStealthTrigger : MonoBehaviour
{
    [SerializeField] private string _tag = "Player";

    private List<ScottsBackup_PlayerStealthMng> _PlayerInStealthObj = new();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_tag))
        {
            var player = other.GetComponent<ScottsBackup_PlayerStealthMng>();
            if (player != null && !_PlayerInStealthObj.Contains(player))
            {
                player.fn_SetInBush();
                _PlayerInStealthObj.Add(player);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_tag))
        {
            var player = other.GetComponent<ScottsBackup_PlayerStealthMng>();
            if (player != null)
            {
                player.fn_SetLeavingBush();

                // Attempt removal and optionally log if not found
                if (!_PlayerInStealthObj.Remove(player))
                {
                    Debug.LogWarning("Player not found in list when exiting bush: " + player.name);
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var item in _PlayerInStealthObj)
        {
            if(item != null)
                item.fn_SetLeavingBush();
        }
    }

}
