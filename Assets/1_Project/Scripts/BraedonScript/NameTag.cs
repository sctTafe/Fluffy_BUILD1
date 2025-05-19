using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// handles the nametags for players 
/// </summary>
public class NameTag : NetworkBehaviour
{
    [SerializeField] private TextMeshPro displayNameText;

    private NetworkVariable<FixedString64Bytes> displayName = new NetworkVariable<FixedString64Bytes>();

    //todo 
    //disable own tag
    //rotate to face player
    //get actual name from playernetworkdata manager
    //in lobby or somewhere make a name input (would need to figure our or make the data manager better i think)
    //(maybe make them put in usernam4e at join code and host)
    

    /*
    //or on spawn each client can load all the name themsleves 
    //also not aure about on network spawn
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) { return; }
        
        PlayerData? playerData = PlayerNetworkDataManager.Instance.GetPlayerDataFromClientId(OwnerClientId);

        if (playerData.HasValue)
        {
            
                

        }
        
        displayName.Value = "test";
    }

    */
    

    private void Awake()
    {
        //disable tag if is owner of own tag (so player don't see own name 
        if (!IsServer) { return; }
        
        displayName.Value = "test";
        //player networkdatamanage GetPlayerDataFromClientId (this.OwnerClientId) give playerData) then get the name to update the networkvariable
    }

    private void OnEnable()
    {
        displayName.OnValueChanged += HandleNameChange;
    }

    private void OnDisable()
    {
        displayName.OnValueChanged -= HandleNameChange;
    }



    private void HandleNameChange(FixedString64Bytes oldDisplayName, FixedString64Bytes newDisplayName)
    {
        displayNameText.text = newDisplayName.ToString();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
