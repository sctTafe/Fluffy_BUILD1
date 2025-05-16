using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

/**
* Script created by Amber to allow the player to pick up items and store them in an inventory
**/

public class PlayerInventory : NetworkBehaviour
{
	public List<string> held_items = new List<string>();

	private bool in_item_hitbox = false;
	private bool in_deposit_hitbox = false;

	private GameObject target;
	private PickUpItem target_data;
	private NetworkObject target_network;
	private DepositItemPoint deposit_point;
	private float pickup_cooldown = 0;
	// private TMP_Text inventory_prompt;

	public RawImage item_slot_1;
	public RawImage item_slot_2;
	public RawImage item_slot_3;

	void Start()
	{
		if(!IsOwner)
			return;

		item_slot_1 = GameObject.Find("item_slot_1").GetComponent<RawImage>();
		item_slot_2 = GameObject.Find("item_slot_2").GetComponent<RawImage>();
		item_slot_3 = GameObject.Find("item_slot_3").GetComponent<RawImage>();

		// inventory_prompt = GameObject.FindWithTag("inventory_prompt").GetComponent<TMP_Text>();
		UpdateUI();
	}

    void Update()
    {
		if(!IsOwner)
		{
			return;
		}

		pickup_cooldown -= Time.deltaTime;

		if(Input.GetKeyDown(KeyCode.E))
		{
			if(in_deposit_hitbox)
			{
				if(held_items.Contains(deposit_point.GetNeededItem()))
				{
					held_items.Remove(deposit_point.GetNeededItem());
					UpdateUI();
					deposit_point.DepositItem();
				}
			}
			else if (in_item_hitbox)
			{
				if(held_items.Count < 3 && pickup_cooldown < 0)
				{
					pickup_cooldown = 2;
					AddToInventory(target_data.GetItem());
					DestroyObjectServerRPC(target_network.NetworkObjectId);
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
	
	[ServerRpc(RequireOwnership = false)]
	private void DestroyObjectServerRPC(ulong to_destroy)
	{
		if(IsServer)
		{
			NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(to_destroy, out NetworkObject target_object);
			if(target_object != null)
			{
				target_object.Despawn();
			}
			else
			{
				Debug.Log("Target object to pick up doesn't exist!");
			}
		}
	}

	public void AddToInventory(string item_name)
	{
		if(held_items.Count < 3)
		{
			held_items.Add(item_name);
			UpdateUI();
		}
	}

	void UpdateUI()
	{
		
		/**
		string new_text = "Held items:";

		foreach(string item in held_items)
		{
			new_text += $"\n{item}";
		}

		inventory_prompt.text = new_text;
		**/	

		if(held_items.Count == 3)
		{
			item_slot_3.texture = Resources.Load<Texture2D>("icons/" + held_items[2]);
		}
		else
		{
			item_slot_3.texture = Resources.Load<Texture2D>("icons/empty");
		}

		if(held_items.Count >= 2)
		{
			item_slot_2.texture = Resources.Load<Texture2D>("icons/" + held_items[1]);
		}
		else
		{
			item_slot_2.texture = Resources.Load<Texture2D>("icons/empty");
		}

		if(held_items.Count >= 1)
		{
			item_slot_1.texture = Resources.Load<Texture2D>("icons/" + held_items[0]);
			Debug.Log("Image loaded!");
		}
		else
		{
			item_slot_1.texture = Resources.Load<Texture2D>("icons/empty");
		}

	}
}
