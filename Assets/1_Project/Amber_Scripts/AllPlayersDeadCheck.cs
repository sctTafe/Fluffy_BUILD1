using UnityEngine;

/// <summary>
/// Runs Locally on all clients
/// </summary>
public class AllPlayersDeadCheck : MonoBehaviour
{
    public float check_peirod = 20;
    private float check_cooldown = 60;


    private void Start()
    {
        check_cooldown = 60; // Inital Check Time - for when players are spawining in
    }

    void Update()
    {
        check_cooldown -= Time.deltaTime;

        if (check_cooldown <= 0)
        {
            CheckForGameEndCondition();
        }

        /*
        // Debug key to give playtester more time when playing alone
        if (Input.GetKeyDown(KeyCode.P))
		{
			check_cooldown = 99999;
		}
        */
    }

    public void fn_ForceEndCheck() => CheckForGameEndCondition();

    void CheckForGameEndCondition()
    {
        check_cooldown = check_peirod;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] mutant = GameObject.FindGameObjectsWithTag("Mutant");

        if (players.Length == 0)
        {
            MainGameManager.Instance.fn_EndGame(false);
        }
        if (mutant.Length == 0)
        {
            MainGameManager.Instance.fn_EndGame(false);
        }
    }

}