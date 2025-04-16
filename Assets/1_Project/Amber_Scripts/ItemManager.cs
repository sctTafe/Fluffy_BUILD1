using UnityEngine;
using Unity.Netcode;

public class ItemManager : NetworkBehaviour
{
	public GameObject[] items;
	public GameObject[] spawn_points;

	public void OnNetworkSpawn()
	{
		items = GameObject.FindGameObjectsWithTag("pick_up_item");
		spawn_points = GameObject.FindGameObjectsWithTag("item_spawn_point");

		Debug.Log(items);

		foreach(GameObject item in items)
		{
			int i = Random.Range(0, spawn_points.Length);
			item.transform.position = spawn_points[i].transform.position;
		}
	}
	
	public void Start()
	{
		items = GameObject.FindGameObjectsWithTag("pick_up_item");
		spawn_points = GameObject.FindGameObjectsWithTag("item_spawn_point");

		Debug.Log(items);

		foreach(GameObject item in items)
		{
			int i = Random.Range(0, spawn_points.Length);
			item.transform.position = spawn_points[i].transform.position;
		}
	}
}
