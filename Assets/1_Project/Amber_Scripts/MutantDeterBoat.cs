using UnityEngine;

public class MutantDeterBoat : MonoBehaviour
{
	private Vector3 boat_position;
	private float distance;
	MutantStamina s;

    void Start()
    {
    	boat_position = GameObject.FindWithTag("boat_escape_point").transform.position;
		s = GetComponent<MutantStamina>();
    }

    void Update()
    {
        distance = Vector3.Distance(boat_position, transform.position);

		if(distance < 15)
		{
			punish_mutant();
		}
    }

	public void punish_mutant()
	{
		s.reduce_stamina(10 * Time.deltaTime);
		// We will also want to have a visual effect here, maybe a vignette?
	}
}
