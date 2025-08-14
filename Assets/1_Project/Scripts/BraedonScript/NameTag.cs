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
    

    private void Awake()
    {
        displayName.OnValueChanged += HandleNameChange;
        var playerManager = PlayerNetworkDataManager.Instance;
        PlayerData? playerData = playerManager.GetPlayerDataFromClientId(OwnerClientId);

        if (playerData.HasValue)
        {
            var data = playerData.Value;
            if (data.playerName.ToString() != null && !data.playerName.IsEmpty)
            {
                displayName.Value = playerData.Value.playerName;
                Debug.Log("----- changed name to: " + playerData.Value.playerName);
            }
            else 
            {
                //player has no name in there playerdata
                displayName.Value = "no name"; 
            }
        }
        else { displayName.Value = "no player data"; }
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        displayName.OnValueChanged -= HandleNameChange;
    }



    private void HandleNameChange(FixedString64Bytes oldDisplayName, FixedString64Bytes newDisplayName)
    {
        Debug.Log("changeName");
        displayNameText.text = newDisplayName.ToString();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //disable own player nametag for them 
        if (IsOwner)
        {
            Debug.Log("this is owner");
            gameObject.SetActive(false);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //make other players name tags look at the the local owners camera
        //reverse's the text rn 
        if (!IsOwner)
        {
            //Debug.Log("Camera.main.gameObject.name");
            gameObject.transform.LookAt(Camera.main.gameObject.transform, Vector3.up);
        }
    }
}
