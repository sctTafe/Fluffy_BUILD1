using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;

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
	
	private ObjectiveManager objective_manager;
	private TMP_Text objective_prompt;

	public void Start()
	{
		objective_prompt = GameObject.FindWithTag(objective_UI_tag).GetComponent<TMP_Text>();
		objective_prompt.text = $"{objective_name} {current_amount.Value} / {amount_needed}";

		objective_manager = GameObject.FindWithTag("ObjectiveManager").GetComponent<ObjectiveManager>();
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
		objective_manager.CompletedObjective();
		Destroy(GameObject.FindWithTag(objective_UI_tag));
	}

	public void UpdateUI()
	{
        objective_prompt.text = $"{objective_name} {current_amount.Value} / {amount_needed}";
    }
}
