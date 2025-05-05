using UnityEngine;
using Unity.Netcode;

public class PlayerStealth : NetworkBehaviour
{
	GameObject geometry;	
	private bool in_bush = false;
	private float time_in_bush = 0;

    void Start()
    {
    	geometry = transform.GetChild(1).gameObject;    
    }

	void Update()
	{
		if(IsOwner)
			return;

		if(in_bush)
		{
			time_in_bush += Time.deltaTime;

			if(time_in_bush > 0.8f)
			{
				geometry.SetActive(false);
				Debug.Log("Player Hidden!");
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
    	if(IsOwner)
			return;
	
		if(other.CompareTag("hide_trigger"))
		{
			in_bush = true;
			time_in_bush = 0;
		}
	}

	void OnTriggerExit(Collider other)
	{
    	if(IsOwner)
			return;
	
		if(other.CompareTag("hide_trigger"))
		{
			geometry.SetActive(true);
			in_bush = false;
			Debug.Log("Player not hidden!");
		}
	}

	
}
