using UnityEngine;

public class ObjectiveManager : Singleton<ObjectiveManager>
{
	public NetworkVariable<int> objectives_completed = new NetworkVariable<int>(0);

	[ServerRpc(RequireOwnership = false)]
	public void CompletedObjectiveServerRPC()
	{
		objectives_completed.Value += 1;
		
		if(objectives_completed.Value >= 3)
		{
			BoatReadyServerRPC();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void BoatReadyServerRPC()
	{
		// Function that runs when all objectives have been completed, telling the boat that it's ready
		Debug.Log("All 3 objectives are complete!");
	}
}
