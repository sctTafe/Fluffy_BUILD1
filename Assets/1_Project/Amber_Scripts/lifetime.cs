using UnityEngine;

public class lifetime : MonoBehaviour
{
	public float life = 0.5f;
	
    void Update()
    {
        life -= Time.deltaTime;

		if(life < 0)
		{
			Destroy(gameObject);
		}
    }
}
