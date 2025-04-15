using UnityEngine;

public class PlayerEscape : MonoBehaviour
{
    private ObjectiveManager objectiveManager;
    private NetworkSceneManager networkSceneManager;

    private void Start()
    {
        objectiveManager = ObjectiveManager.Instance;
        networkSceneManager = NetworkSceneManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("boat_escape_point") && objectiveManager.CanPlayersEscape())
        {
            MainGameManager.Instance.DespawnPlayers();
            networkSceneManager.fn_GoToScene("4_Lobby");
            Debug.Log("EndGame");
        }
    }
}
