using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using System.Collections;

public class DepositItemPoint : NetworkBehaviour
{
	/**
	* Code for the deposit item point.
	* Allows the players to deposit items to increase an objective's completion.
	* Has an amount_needed variable, item_needed variable.
	* Has a network variable to store the progress towards the objective current_amount
	*
	* Don't forget to tag the deposit point deposit_point !
	**/

	public int amount_needed = 3;
	public string item_needed = "yellow_test_item";
	public NetworkVariable<int> current_amount = new NetworkVariable<int>(0);
	public string objective_name = "Deposit petrol containers";
	public string objective_UI_tag = "objective_prompt";
	
	[SerializeField] private TMP_Text objective_prompt;

    public void Start()
	{
        // Disable Self If Not Owner
		/**
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }
		**/
		UpdateUI();
		// objective_manager = GameObject.FindWithTag("ObjectiveManager").GetComponent<ObjectiveManager>();
	}

	void Update()
	{
		/**
		if(objective_prompt == null && Time.frameCount % 10 == 0)
		{
			objective_prompt = GameObject.FindWithTag(objective_UI_tag).GetComponent<TMP_Text>();
			
			if(objective_prompt != null)
			{
				UpdateUI();
			}
		}
		**/

		// DEV TESTING
		if(Input.GetKeyDown(KeyCode.O))
		{
			DepositItem();
		}
	}


    private void OnEnable()
    {
		current_amount.OnValueChanged += HandleOnValueChange;
    }

    private void OnDisable()
    {
        current_amount.OnValueChanged -= HandleOnValueChange;
    }

	/// <summary>
	/// Runs on all clinets when value changes
	/// </summary>
    private void HandleOnValueChange(int previousValue, int newValue)
    {
        UpdateUI();
    }

    public string GetNeededItem()
	{
		return item_needed;
	}

	public void DepositItem()
	{
        IncreaseAmountServerRPC();
	}

	[ServerRpc(RequireOwnership = false)]
	private void IncreaseAmountServerRPC()
	{
		current_amount.Value += 1;

		Debug.Log($"Deposited item! {current_amount.Value} / {amount_needed}");

		if(current_amount.Value >= amount_needed)
		{
			BroadcastObjectiveComplete();
		}
	}

	public void BroadcastObjectiveComplete()
	{
		ObjectiveManager.Instance.CompletedObjectiveServerRPC();
		ObjectiveManager.Instance.CompletedObjective();
		Destroy(GameObject.FindWithTag(objective_UI_tag));
	}

	public void UpdateUI()
	{
		if(objective_prompt != null)
		{
        	objective_prompt.text = $"{objective_name} {current_amount.Value} / {amount_needed}";
		}
    }
}
