using UnityEngine;
using Unity.Netcode;
using TMPro;

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

	private TMP_Text objective_prompt;

	public void Start()
	{
		objective_prompt = GameObject.FindWithTag("objective_prompt").GetComponent<TMP_Text>();
		objective_prompt.text = $"{objective_name} {current_amount.Value} / {amount_needed}";
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

		objective_prompt.text = $"{objective_name} {current_amount.Value} / {amount_needed}";

		if(current_amount.Value >= amount_needed)
		{
			BroadcastObjectiveComplete();
		}
	}

	public void BroadcastObjectiveComplete()
	{
		Debug.Log("Unfinished function to indicate an objective is complete");
		
		//Put the code to call that an objective is complete here
	}
}
