using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using System.Collections;

public class DepositItemPoint : NetworkBehaviour
{
	public Action _OnDepositItem;

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
	public ResoruceType resoruceType;
	
    [SerializeField] private TMP_Text objective_prompt;

    bool _isCompleted = false;

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
    public void UpdateUI()
    {
        if (objective_prompt != null)
        {
            objective_prompt.text = $"{objective_name} {current_amount.Value} / {amount_needed}";
        }
    }


    public string GetNeededItem()
	{
		return item_needed;
	}

	public void DepositItem()
	{
        IncreaseAmountServerRPC();
		_OnDepositItem?.Invoke();

    }

	[ServerRpc(RequireOwnership = false)]
	private void IncreaseAmountServerRPC()
	{
		// Only Increase the number if its below the needed amount
        if (current_amount.Value < amount_needed)
            current_amount.Value += 1;

		// Max Value is the target amount
		if (current_amount.Value > amount_needed)
			current_amount.Value = amount_needed;

        Debug.Log($"Deposited item! {current_amount.Value} / {amount_needed}");
		
		if(_isCompleted == false)
		{
            _isCompleted = true;
            if (current_amount.Value == amount_needed)
            {
                BroadcastObjectiveComplete();
            }
        }
	}

	public void BroadcastObjectiveComplete()
	{
		ObjectiveManager.Instance.fn_CompletedObjective_ServerONLY(resoruceType);
		//Destroy(GameObject.FindWithTag(objective_UI_tag));  // THis only works on host, it is only called on the server i.e. host
	}


}
