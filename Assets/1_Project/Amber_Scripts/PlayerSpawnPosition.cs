using UnityEngine;

public class PlayerSpawnPosition : MonoBehaviour
{
	public Vector3 spawn_pos;
	public float random_offset;

    void Start()
    {
		spawn_pos = new Vector3(spawn_pos.x + Random.Range(-random_offset, random_offset), spawn_pos.y, spawn_pos.z + Random.Range(-random_offset, random_offset));

		transform.position = spawn_pos;
        
    }
}
