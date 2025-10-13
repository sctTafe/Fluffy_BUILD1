
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// Summary of update:
/// 😤 Frustrated / Honest (informal dev log style)
///     Updated to fix the exact same problem that was already resolved six weeks ago — reintroduced when someone rewrote the system and brought 
///     the issue back.
/// 🧑‍💻 Professional but direct (for commit message or changelog)
///     Reapplied previous fix from six weeks ago after the issue was reintroduced during a system rewrite.
/// 🧾 Neutral summary (for documentation)
///     Fixed a regression of an issue previously resolved six weeks ago, which was reintroduced following a system rewrite.
/// 
/// </summary>
public class ScottsBackup_MapObjectStealthTrigger : MonoBehaviour
{
    [SerializeField] private string _tag = "Player";

    //private List<ScottsBackup_PlayerStealthMng> _PlayerInStealthObj = new();

    private List<PlayerStealth> _playerStealth_IzzacReWrite = new();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_tag))
        {
            //var player = other.GetComponent<ScottsBackup_PlayerStealthMng>();
            //if (player != null && !_PlayerInStealthObj.Contains(player))
            //{
            //    player.fn_SetInBush();
            //    _PlayerInStealthObj.Add(player);
            //}

            var player_2 = other.GetComponent<PlayerStealth>();
            if (player_2 != null && !_playerStealth_IzzacReWrite.Contains(player_2))
            {
                _playerStealth_IzzacReWrite.Add(player_2);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_tag))
        {
            //var player = other.GetComponent<ScottsBackup_PlayerStealthMng>();
            //if (player != null)
            //{
            //    player.fn_SetLeavingBush();

            //    // Attempt removal and optionally log if not found
            //    if (!_PlayerInStealthObj.Remove(player))
            //    {
            //        Debug.LogWarning("Player not found in list when exiting bush: " + player.name);
            //    }
            //}

            var player_2 = other.GetComponent<PlayerStealth>();
            if (player_2 != null)
            {
                _playerStealth_IzzacReWrite.Remove(player_2);
            }

        }
    }

    private void OnDestroy()
    {
        //foreach (var item in _PlayerInStealthObj)
        //{
        //    if(item != null)
        //        item.fn_SetLeavingBush();
        //}

        foreach (var item in _playerStealth_IzzacReWrite)
        {
            if (item != null)
                item.fn_setUnhide();
        }
    }

}
