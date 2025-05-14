using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;
using System;


public class ScottsBackup_ActionPlayerItemPickUp : PlayerActionBase, IHudAbilityBinder
{
    private const bool ISDEBUGGING = true;

    public event Action<float> OnCooldownWithLengthTriggered;
    public event Action OnCooldownCanceled;


    public List<string> held_items = new List<string>();

    private bool in_item_hitbox = false;
    private bool in_deposit_hitbox = false;

    private GameObject target;
    private PickUpItem target_data;
    private NetworkObject target_network;
    private DepositItemPoint deposit_point;
    // private TMP_Text inventory_prompt;

    public RawImage item_slot_1;
    public RawImage item_slot_2;
    public RawImage item_slot_3;

    bool _Input;

    void Start()
    {
        // Disable Self If Not Owner
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }

        TryGetRefs();
        UpdateUI();
    }


    void Update()
    {

        // Resoruce Load Check
        if (item_slot_1 == null)
        {
            TryGetRefs();
            Debug.LogWarning("PlayerInventory Unable to find UI Refs!");
            return;
        }

        if (_Input)
        {
            _Input = false;

            OnCooldownWithLengthTriggered?.Invoke(0.5f);

            if (in_deposit_hitbox)
            {
                if (ISDEBUGGING) Debug.Log($"ScottsBackup_ActionPlayerItemPickUp: in_deposit_hitbox ");

                if (held_items.Contains(deposit_point.GetNeededItem()))
                {

                    if (ISDEBUGGING) Debug.Log($"ScottsBackup_ActionPlayerItemPickUp: held_items ");

                    held_items.Remove(deposit_point.GetNeededItem());
                    UpdateUI();
                    deposit_point.DepositItem();
                }
            }

            else if (in_item_hitbox)
            {
                if (held_items.Count < 3)
                {
                    AddToInventory(target_data.GetItem());
                    DestroyObjectServerRPC(target_network.NetworkObjectId);
                    in_item_hitbox = false;
                }
                else
                {
                    //We need to add a way to allow the player to drop items, or allow the player to hold multiple items
                    Debug.Log("You are already carrying an item");
                }
            }
        }
    }

    public override bool fn_ReceiveActivationInput(bool b)
    {
        _Input = true;
        //Not Setup in theis version
        return false;
    }


    private void TryGetRefs()
    {
        GameObject go;

        go = GameObject.Find("item_slot_1");
        if (go != null)
            item_slot_1 = go.GetComponent<RawImage>();

        go = GameObject.Find("item_slot_2");
        if (go != null)
            item_slot_2 = go.GetComponent<RawImage>();

        go = GameObject.Find("item_slot_3");
        if (go != null)
            item_slot_3 = go.GetComponent<RawImage>();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("pick_up_item"))
        {
            in_item_hitbox = true;
            target = other.gameObject;
            target_data = target.GetComponent<PickUpItem>();
            target_network = target.GetComponent<NetworkObject>();
        }

        if (other.CompareTag("deposit_point"))
        {
            in_deposit_hitbox = true;
            deposit_point = other.gameObject.GetComponent<DepositItemPoint>();
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("pick_up_item"))
        {
            in_item_hitbox = false;
        }

        if (other.CompareTag("deposit_point"))
        {
            in_deposit_hitbox = false;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void DestroyObjectServerRPC(ulong to_destroy)
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(to_destroy, out NetworkObject target_object);
            if (target_object != null)
            {
                target_object.Despawn();
            }
        }
    }


    public void AddToInventory(string item_name)
    {
        if (held_items.Count < 3)
        {
            held_items.Add(item_name);
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        // Resoruce Load Check
        if (item_slot_1 == null)
        {
            TryGetRefs();
            Debug.LogWarning("PlayerInventory Unable to find UI Refs!");
            return;
        }


        if (held_items.Count == 3)
        {
            item_slot_3.texture = Resources.Load<Texture2D>("icons/" + held_items[2]);
        }
        else
        {
            item_slot_3.texture = Resources.Load<Texture2D>("icons/empty");
        }

        if (held_items.Count >= 2)
        {
            item_slot_2.texture = Resources.Load<Texture2D>("icons/" + held_items[1]);
        }
        else
        {
            item_slot_2.texture = Resources.Load<Texture2D>("icons/empty");
        }

        if (held_items.Count >= 1)
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
