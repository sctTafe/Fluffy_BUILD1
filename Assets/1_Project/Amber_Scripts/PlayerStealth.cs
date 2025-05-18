using UnityEngine;
using Unity.Netcode;
using TMPro;
using FMODUnity;

public class PlayerStealth : NetworkBehaviour
{
	GameObject geometry;	
	private bool in_bush = false;
	private float time_in_bush = 0;
	private float force_reveal = 0;
	private TMP_Text stealth_prompt;

	private bool played_sound = false;
	
	public EventReference hide_sound;

    void Start()
    {
    	geometry = transform.GetChild(3).gameObject;    
		if(IsOwner)
		{
			GameObject stealthObject = GameObject.FindWithTag("stealth_prompt");
            if (stealthObject)
				stealth_prompt = stealthObject.GetComponent<TMP_Text>();
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
					if(!played_sound)
					{
						play_hide_sound();
						played_sound = true;
					}
				}
			}
		}
		else
		{
			played_sound = false;
		}

		force_reveal -= Time.deltaTime;

		// Doesn't show revealed text, probably needs to be an RPC
		if(IsOwner && force_reveal > 0)
		{
			stealth_prompt.text = "[ Revealed! ]";
		}

	}

	void play_hide_sound()
	{
		RuntimeManager.PlayOneShot(hide_sound, transform.position);
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
		if(!IsOwner)
		{
			force_reveal = 10;
			geometry.SetActive(true);
		}
		
		force_reveal_ServerRPC();

	}

	[ServerRpc(RequireOwnership = false)]
	private void force_reveal_ServerRPC()
	{
		client_reveal_ClientRPC();
	}

	[Rpc(SendTo.ClientsAndHost)]
	private void client_reveal_ClientRPC()
	{
		if(IsOwner)
		{
			Debug.Log("Server RPC to reveal player was called");
			force_reveal = 10;
			stealth_prompt.text = "[ Revealed! ]";
		}
	}

}
