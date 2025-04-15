using UnityEngine;
using Unity.Netcode;
using TMPro;

/**
* Script created by Amber to allow the player to pick up items and store them in an inventory
**/

public class PlayerInventory : NetworkBehaviour
{
	public string held_item = "none";

	private bool in_item_hitbox = false;
	private bool in_deposit_hitbox = false;

	private GameObject target;
	private PickUpItem target_data;
	private NetworkObject target_network;
	private DepositItemPoint deposit_point;
	private TMP_Text inventory_prompt;

	void Start()
	{
		inventory_prompt = GameObject.FindWithTag("inventory_prompt").GetComponent<TMP_Text>();
		inventory_prompt.text = $"Held item:\n{held_item}";
	}

    void Update()
    {
		if(Input.GetKeyDown(KeyCode.E))
		{
			if(in_deposit_hitbox)
			{
				if(held_item == deposit_point.GetNeededItem())
				{
					held_item = "none";
					inventory_prompt.text = $"Held item:\n{held_item}";
					deposit_point.DepositItem();
				}
			}
			else if (in_item_hitbox)
			{
				if(held_item == "none")
				{
					held_item = target_data.GetItem();
					inventory_prompt.text = $"Held item:\n{held_item}";
					DestroyObjectServerRPC(target_network.NetworkObjectId);
					Debug.Log(held_item);
				}
				else
				{
					//We need to add a way to allow the player to drop items, or allow the player to hold multiple items
					Debug.Log("You are already carrying an item");
				}
			}
		}
    }

	void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("pick_up_item"))
		{
			in_item_hitbox = true;
			target = other.gameObject;	
			target_data = target.GetComponent<PickUpItem>();
			target_network = target.GetComponent<NetworkObject>();
		}
		
		if(other.CompareTag("deposit_point"))
		{
			in_deposit_hitbox = true;
			deposit_point = other.gameObject.GetComponent<DepositItemPoint>();
		}
	}

	void OnTriggerExit(Collider other)
	{
		if(other.CompareTag("pick_up_item"))
		{
			in_item_hitbox = false;
		}

		if(other.CompareTag("deposit_point"))
		{
			in_deposit_hitbox = false;
		}
	}
	
	// [Rpc.SendTo(Server)]
	/**
	[ServerRpc(RequireOwnership = false)]
	private void DestroyObjectServerRPC(ulong to_destory)
	{
		Server_DestroyObjectServerRPC(to_destroy);
	}
	**/
	
	[ServerRpc(RequireOwnership = false)]
	private void DestroyObjectServerRPC(ulong to_destroy)
	{
		if(IsServer)
		{
			NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(to_destroy, out NetworkObject target_object);
			target_object.Despawn();
		}
	}

	public void AddToInventory(string item_name)
	{
		held_item = item_name;
	}
}
