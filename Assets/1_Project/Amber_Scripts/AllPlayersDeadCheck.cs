using UnityEngine;

public class AllPlayersDeadCheck : MonoBehaviour
{
	private float check_cooldown = 6;

    void Update()
    {
		check_cooldown -= Time.deltaTime;
		
		if(check_cooldown <= 0)
		{
			check_cooldown = 6;

			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

			if(players.Length == 0)
			{
				MainGameManager.Instance.fn_EndGame(false);
			}
		}

		// Debug key to give playtester more time when playing alone
		if(Input.GetKeyDown(KeyCode.P))
		{
			check_cooldown = 99999;
		}
        
    }
}
