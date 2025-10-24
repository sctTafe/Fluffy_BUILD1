using UnityEngine;
using Unity.Netcode;
using System;

/// <summary>
/// 
/// Keeps Track of the Objectives Competed
/// 
/// 
/// </summary>
/// 
public enum ResoruceType
{
	fuel,
	food,
	wood
}
public class ObjectiveManager : NetworkSingleton<ObjectiveManager>
{
	public Action _OnObjectivesComplete;
	
	public NetworkVariable<int> objectivesCompleted_NV = new NetworkVariable<int>(0);
	
	// Server Only Variables
	int objectivesCompleted_ServerVariable = 0;
	bool _FoodDone = false;
	bool _FuelDone = false;
	bool _WoodDone = false;



    /// <summary>
    /// Checks Network Variable Value for how many objectives have been completed
    /// </summary>
    public bool fn_CanPlayersEscape()
    {
        // return (objectives_completed.Value >= 3);
        var count = objectivesCompleted_NV.Value;
        Debug.Log($"Players have completed {count}");
        return (objectivesCompleted_NV.Value >= 3);
    }

    // Server Only
    public void fn_CompletedObjective_ServerONLY(ResoruceType resoruceType)
	{
		if(!IsServer)
			return; 

		if(CanCountTheObjectiveUpdate_FIX(resoruceType) == false)
			return;

		objectivesCompleted_ServerVariable += 1;
        objectivesCompleted_NV.Value = objectivesCompleted_ServerVariable;


        Debug.Log("Objective Complete NonRPC!");
		
		if(objectivesCompleted_ServerVariable >= 3)
		{
            BoatIsReady_ClientRPC();
            TriggerBoatThing_ServerRPC();
        }

	}


    [ServerRpc]
    private void TriggerBoatThing_ServerRPC()
    {
        Debug.Log("TriggerBoatThing_ServerRPC Called");
        GetToTheBoat.Instance.fn_TriggerGetToTheBoat();
    }



	[ClientRpc]
	private void BoatIsReady_ClientRPC()
	{
        // Function that runs when all objectives have been completed, telling the boat that it's ready
        Debug.Log("All 3 objectives are complete!");
        _OnObjectivesComplete?.Invoke();
    }

    private bool CanCountTheObjectiveUpdate_FIX(ResoruceType resoruceType)
    {
        if (resoruceType == ResoruceType.fuel)
        {
            if (_FuelDone == false)
            {
                _FuelDone = true;
                return true;
            }
        }
        if (resoruceType == ResoruceType.food)
        {
            if (_FoodDone == false)
            {
                _FoodDone = true;
                return true;
            }
        }
        if (resoruceType == ResoruceType.wood)
        {
            if (_WoodDone == false)
            {
                _WoodDone = true;
                return true;
            }
        }

        // Resrouce Already Counted
        return false;
    }


}
