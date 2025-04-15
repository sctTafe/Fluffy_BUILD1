using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
//using System.Diagnostics;

public class PlayerHealth : NetworkBehaviour
{
    //Creating player health network variable
	public NetworkVariable<float> player_health = new NetworkVariable<float>(1);

	private GameObject health_bar;

	void Start()
	{
		health_bar = GameObject.FindGameObjectWithTag("HealthBar");
	}

	void OnEnable()
	{
		player_health.OnValueChanged += RespondToEvent;
		//SetPlayerHealthServerRPC(1);
		player_health.Value = 1;
	}

	void OnDisable()
	{
		player_health.OnValueChanged -= RespondToEvent;
	}


	void RespondToEvent(float old, float newValue){
		ProcessHealthUpdate(old, newValue);
	}

	override public void OnNetworkSpawn()
    {
        Debug.Log("Player Health OnNetworkSpawn Called");

        Start_IsClient();
        Start_IsServer();
	}
    
	[Rpc(SendTo.ClientsAndHost)]
	void SetPlayerHealthServerRPC(float new_health){
		player_health.Value = new_health;
	}

	private void Start_IsServer()
    {
        if (!IsHost)
            return;

        player_health.Value = 1; //Set player health to 1 On Start
		Debug.Log("(Client) Player health value: " + player_health.Value);

    }

    private void Start_IsClient()
    {
        if (!IsClient)
            return;

		ProcessHealthUpdate(1, player_health.Value);
		Debug.Log("(Client) Player health value: " + player_health.Value);
    }

	//Runs on all local clients when called
	private void ProcessHealthUpdate(float previous_value, float new_value)
	{
		//Check if health is 0
		if(new_value <= 0)
		{
			KillPlayer();
		}

		//Update UI
		UpdatePlayerUI(new_value);
	}

	void UpdatePlayerUI(float new_value)
	{
		health_bar.transform.localScale = new Vector3(new_value / 1, 1, 1);
	}

	void KillPlayer()
	{
		Debug.Log("This is an unused function that will handle killing the player when they run out of health once we have that code implemented.");
	}

	public void ChangePlayerHealth(float change_amount)
	{
		ChangePlayerHealthServerRPC(change_amount);
	}

	[Rpc(SendTo.ClientsAndHost)]
	private void ChangePlayerHealthServerRPC(float change_amount)
	{
		player_health.Value += change_amount;
		Debug.Log(player_health.Value);
	}

}
