using UnityEngine;

public class MutantRevealPlayer : MonoBehaviour
{
	MutantStamina s;
	GameObject[] players;
	private float cooldown = 0;

	void Start()
	{
		s = GetComponent<MutantStamina>();
	}

	void Update()
	{
		cooldown -= Time.deltaTime;		

		if(cooldown <= 0 && Input.GetKeyDown(KeyCode.Q) && s.get_stamina() > 40)
		{
			s.reduce_stamina(40);
			cooldown = 10;

			players = GameObject.FindGameObjectsWithTag("Player");

			foreach(GameObject player in players)
			{
				if(Vector3.Distance(player.transform.position, transform.position) < 15)
				{
					player.GetComponent<PlayerStealth>().force_unhide();
				}
			}
		}
	}
}
