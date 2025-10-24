using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEscape : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("boat_escape_point") && ObjectiveManager.Instance.fn_CanPlayersEscape())
        {
            Debug.Log("PlayerEscape: EndGame");
            MainGameManager.Instance.fn_EndGame(true);        
        }
    }


}
