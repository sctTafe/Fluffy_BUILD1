using UnityEngine;
using Unity.Netcode;

public class Tutorial : NetworkBehaviour
{
	[SerializeField] GameObject arrow_prefab;

	GameObject arrow;

	[SerializeField] Vector3[] key_locations;

	int i = 0;

    void Start()
    {
		if(!IsOwner)
		{
			return;
		}

		arrow = Instantiate(arrow_prefab, transform);
    }

    void Update()
    {
		if(!IsOwner)
		{
			return;
		}

		if(i < key_locations.Length)
		{
			update_objective();
		}
    }
	
	void update_objective()
	{
		arrow.transform.LookAt(key_locations[i]);
		
		Debug.Log("Turotial progress: " + i);
       
	   	if(Time.frameCount % 5 == 0 && Vector3.Distance(transform.position, key_locations[i]) < 3)
		{
			i += 1;

			if(key_locations.Length == i)
			{
				Destroy(arrow);
			}
		}

	}
}
