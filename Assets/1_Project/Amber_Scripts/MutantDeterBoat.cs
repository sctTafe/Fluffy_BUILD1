using UnityEngine;

public class MutantDeterBoat : MonoBehaviour
{
	private Vector3 boat_position;
	private float distance;
	ScottsBackup_ResourceMng s;

    void Start()
    {
		GameObject boat = GameObject.FindWithTag("boat_escape_point");
		if (boat != null)
        boat_position = boat.transform.position;
		s = GetComponent<ScottsBackup_ResourceMng>();
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
		s.fn_TryReduceValue(2 * Time.deltaTime);
		// We will also want to have a visual effect here, maybe a vignette?
	}
}
