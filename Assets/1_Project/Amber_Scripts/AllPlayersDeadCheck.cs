using UnityEngine;

public class AllPlayersDeadCheck : MonoBehaviour
{
	public float check_peirod = 20;
	private float check_cooldown = 60;

    void Update()
    {
		check_cooldown -= Time.deltaTime;
		
		if(check_cooldown <= 0)
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

		// Debug key to give playtester more time when playing alone
		if(Input.GetKeyDown(KeyCode.P))
		{
			check_cooldown = 99999;
		}
        
    }
}
