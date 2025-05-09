using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerStealth : NetworkBehaviour
{
	GameObject geometry;	
	private bool in_bush = false;
	private float time_in_bush = 0;
	private float force_reveal = 0;
	private TMP_Text stealth_prompt;

    void Start()
    {
    	geometry = transform.GetChild(3).gameObject;    
		if(IsOwner)
		{
			stealth_prompt = GameObject.FindWithTag("stealth_prompt").GetComponent<TMP_Text>();
		}
    }

	void Update()
	{
		if(in_bush)
		{
			time_in_bush += Time.deltaTime;

			if(time_in_bush > 0.8f && force_reveal <= 0)
			{
				if(!IsOwner)
				{
					geometry.SetActive(false);
				}
				else
				{
					stealth_prompt.text = "[ Hidden! ]";
				}
			}
		}

		force_reveal -= Time.deltaTime;

		// Doesn't show revealed text, probably needs to be an RPC
		if(IsOwner && force_reveal > 0)
		{
			stealth_prompt.text = "[ Revealed! ]";
		}

	}

	void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("hide_trigger"))
		{
			in_bush = true;
			time_in_bush = 0;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if(other.CompareTag("hide_trigger"))
		{
			in_bush = false;

			if(!IsOwner)
			{
				geometry.SetActive(true);
			}
			else
			{
				stealth_prompt.text = "";
			}
		}
	}

	// Forces the player to reveal for 10 seconds
	// Called with mutant scan attack

	// Reveal probably needs to be an RPC to get this to work
	public void force_unhide()
	{
		force_reveal = 10;
		if(!IsOwner)
		{
			geometry.SetActive(true);
		}
		else
		{
			stealth_prompt.text = "[ Revealed! ]";
		}

	}
}
