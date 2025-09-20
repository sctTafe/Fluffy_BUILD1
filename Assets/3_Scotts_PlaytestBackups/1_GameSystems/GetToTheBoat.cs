using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class GetToTheBoat : NetworkBehaviour
{
    public bool _IsInputCheatEnabled = false;

    UnityEvent _OnObjectivesComplete;

    [Header("Stuff To Turn Off")]
    [SerializeField] GameObject _rootGameObjectOfAllPickups;
    public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    [Header("Stuff To Turn On")]
    [SerializeField] GameObject _GetToTheBoatObjects;


    void Start()
    {
        ObjectiveManager.Instance._OnObjectivesComplete += HandleOnObjectivesComplete;
    }
    void OnDisable()
    {
        ObjectiveManager.Instance._OnObjectivesComplete -= HandleOnObjectivesComplete;
    }

    private void Update()
    {
        if (_IsInputCheatEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Alpha9)) // Number 9 on the main keyboard
            {
                HandleOnObjectivesComplete();
            }
        }
    }

    // Priamary / Main Function
    private void HandleOnObjectivesComplete()
    {
        ServerRpc();
    }


    // ServerRpc - called from client, runs on server
    [Rpc(SendTo.Server)]
    private void ServerRpc()
    {
        ClientRpc();
    }

    // ClientRpc - called from server, runs on all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void ClientRpc()
    {
        GetToTheBoat_Main();
    }

    private void GetToTheBoat_Main()
    {
        // Disable Effects on pickups Round the Island
        DisableAllParticleSystems();

        // Enable Boat Effects
        _GetToTheBoatObjects.SetActive(true);

        // Message All Player (Inc Mutant)
        HUD_PopUpMessages_Singelton.Instance.fn_PopupMessage("Boat Reeady! Get to the boat", HUD_PopUpMessages_Singelton.PopupStyle.Bounce, 3f);

        _OnObjectivesComplete?.Invoke();
    }

    /// <summary>
    /// Finds all ParticleSystems under the root (including inactive ones).
    /// </summary>
    public void FindAllParticleSystems()
    {
        particleSystems.Clear();
        if (_rootGameObjectOfAllPickups != null)
        {
            particleSystems.AddRange(_rootGameObjectOfAllPickups.GetComponentsInChildren<ParticleSystem>(true));
        }
    }

    /// <summary>
    /// Disables (stops) all ParticleSystems found in the list.
    /// </summary>
    public void DisableAllParticleSystems()
    {
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

}










// In Editor Button
#if UNITY_EDITOR

[CustomEditor(typeof(GetToTheBoat))]
public class ParticleSystemManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector first
        DrawDefaultInspector();

        GetToTheBoat manager = (GetToTheBoat)target;

        // Add a button for populating the list
        if (GUILayout.Button("Populate Particle System List"))
        {
            manager.FindAllParticleSystems();
            EditorUtility.SetDirty(manager); // Mark object dirty so changes save
        }

        if (GUILayout.Button("Disable All Particles"))
        {
            manager.DisableAllParticleSystems();
        }


    }
}
#endif


