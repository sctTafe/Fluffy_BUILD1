using UnityEngine;
using Unity.Netcode;

public class MutantStamina : NetworkBehaviour
{
	private float stamina = 100;
	private GameObject stamina_bar;

	void Start()
	{
		if(!IsOwner)
			return;

		stamina_bar = GameObject.FindWithTag("stamina_bar");
	}

    void Update()
    {
		if(!IsOwner)
			return;

		stamina += Time.deltaTime * 5;        
		stamina = Mathf.Clamp(stamina,0,100);

		stamina_bar.transform.localScale = new Vector3(stamina / 100, 1, 1);
    }

	public void reduce_stamina(float amount)
	{
		stamina -= amount;
	}

	public float get_stamina()
	{
		return stamina;
	}
}
