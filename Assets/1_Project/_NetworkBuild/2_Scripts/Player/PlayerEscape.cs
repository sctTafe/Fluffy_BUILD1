using UnityEngine;

public class PlayerEscape : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("boat_escape_point") && ObjectiveManager.Instance.CanPlayersEscape())
        {
            MainGameManager.Instance.DespawnPlayers();
            NetworkSceneManager.Instance.fn_GoToScene("4_Lobby");
            Debug.Log("EndGame");
        }
    }
}
