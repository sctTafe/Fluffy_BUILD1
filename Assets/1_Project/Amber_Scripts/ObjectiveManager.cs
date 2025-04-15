using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
	public int objectives_completed = 0;

	public void CompletedObjective()
	{
		objectives_completed += 1;
		
		if(objectives_completed >= 3)
		{
			BoatReady();
		}
	}

	private void BoatReady()
	{
		// Function that runs when all objectives have been completed, telling the boat that it's ready
		Debug.Log("All 3 objectives are complete!");
	}
}
