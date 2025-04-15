using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEscape : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("boat_escape_point") && ObjectiveManager.Instance.CanPlayersEscape())
        {
            MainGameManager.Instance.fn_DespawnPlayers();

            StartCoroutine(WaitAndDoSomething());

            NetworkSceneManager.Instance.fn_GoToScene("4_Lobby");
            Debug.Log("EndGame");
        }
    }

    IEnumerator WaitAndDoSomething()
    {
        Debug.Log("Waiting for 1.5 seconds...");
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Done waiting!");
        // You can add any logic you want to happen after the wait here
    }
}
