using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ItemManager : NetworkBehaviour
{
    //public GameObject[] items;
    //public GameObject[] spawn_points;

	public Transform[] objectiveGroups;
	public Transform[] spawnPointGroups;

	public override void OnNetworkSpawn()
	{
		if (!IsServer)
			return;

        Debug.Log("ItemManager - OnNetworkSpawn");

        if (objectiveGroups.Length != spawnPointGroups.Length)
        {
            Debug.LogError("The length of objectiveGroups needs to be the same as spawnPointGroups: " + this.gameObject.name);
        }


		//      items = GameObject.FindGameObjectsWithTag("pick_up_item");
		//spawn_points = GameObject.FindGameObjectsWithTag("item_spawn_point");


		//      foreach (GameObject item in items)
		//{
		//	int i = Random.Range(0, spawn_points.Length);
		//	item.transform.position = spawn_points[i].transform.position;
		//}

		foreach(Transform objective_group in objectiveGroups)
		{
			List<GameObject> objectiveList = new List<GameObject>();
			foreach(Transform objective in objective_group)
			{
                objectiveList.Add(objective.gameObject);
			}
		}

		for (int i = 0; i < spawnPointGroups.Length; ++i)
		{
			Transform spawnPointGroup = spawnPointGroups[i];
			Transform objectiveGroup = objectiveGroups[i];

			List<Transform> spawnPoints = new List<Transform>();
			List<Transform> objectives = new List<Transform>();

			foreach (Transform spawnPoint in spawnPointGroup)
			{
				spawnPoints.Add(spawnPoint);
			}

			foreach (Transform objective in objectiveGroup)
			{
				int spawnIndex = Random.Range(0, spawnPoints.Count);
				objective.position = spawnPoints[spawnIndex].position;
				spawnPoints.RemoveAt(spawnIndex);
			}
		}


    }
	
	//public void Start()
	//{
	//	items = GameObject.FindGameObjectsWithTag("pick_up_item");
	//	spawn_points = GameObject.FindGameObjectsWithTag("item_spawn_point");

	//	Debug.Log(items);

	//	foreach(GameObject item in items)
	//	{
	//		int i = Random.Range(0, spawn_points.Length);
	//		item.transform.position = spawn_points[i].transform.position;
	//	}
	//}
}
