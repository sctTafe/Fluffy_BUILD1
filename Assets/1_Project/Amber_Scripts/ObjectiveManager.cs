using UnityEngine;
using Unity.Netcode;

public class ObjectiveManager : NetworkSingleton<ObjectiveManager>
{
	public NetworkVariable<int> objectives_completed = new NetworkVariable<int>(0);

	[ServerRpc(RequireOwnership = false)]
	public void CompletedObjectiveServerRPC()
	{
		objectives_completed.Value += 1;
		
		Debug.Log("Objective Complete!");
		
		if(objectives_completed.Value >= 3)
		{
			BoatReady();
		}
	}

	private void BoatReady()
	{
		// Function that runs when all objectives have been completed, telling the boat that it's ready
		Debug.Log("All 3 objectives are complete!");
	}

	public bool CanPlayersEscape()
	{
		return (objectives_completed.Value >= 3);
	}
}
