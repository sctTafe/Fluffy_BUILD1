using Unity.Netcode;
using UnityEngine;

public class ScottsBackup_PlayerMaterialSwapper : NetworkBehaviour
{
    [Header("Player Texture Matrials")]
    [SerializeField] private GameObject _gameObjectToApplyTo_Body;
    [SerializeField] private GameObject _gameObjectToApplyTo_Head;

    [Header("Player Texture Matrials")]
    [SerializeField] private Material[] _materials;


    private Renderer objRenderer_Body;
    private Renderer objRenderer_Head;

    void Awake()
    {
        if(_gameObjectToApplyTo_Body == null)
        {
            Debug.LogWarning($"ScottsBackup_PlayerMaterialSwapper - No _gameObjectToApplyTo_Body assigned on {name}!");
            return;
        }
        if (_gameObjectToApplyTo_Head == null)
        {
            Debug.LogWarning($"ScottsBackup_PlayerMaterialSwapper - No _gameObjectToApplyTo_Head assigned on {name}!");
            return;
        }

        objRenderer_Body = _gameObjectToApplyTo_Body.GetComponent<Renderer>();
        objRenderer_Head = _gameObjectToApplyTo_Head.GetComponent<Renderer>();
    }



    [ClientRpc]
    public void SwapPlayerMaterialClientRpc(int index)
    {
        fn_SwapPlayerMaterailTo(index);
    }


    public void fn_SwapPlayerMaterailTo(int index)
    {
        if(index >= _materials.Length)
        {
            Debug.LogError($"Cannont Find Indexed Material on {name}! ");
            return;
        }

        if (index >= 0 && index < _materials.Length)
        {
            Debug.Log($"fn_SwapPlayerMaterailTo called on {gameObject.name}, set to material {index}");
            objRenderer_Head.material = _materials[index];
            objRenderer_Body.material = _materials[index];
        }
    }


    //private void SwopMatialOnObject(GameObject gameObject, int index)
    //{
        
    //    objRenderer_Body.material = mat;
    //}






    /// <summary>
    /// Swap to one of the random materials
    /// </summary>
    public void fn_SwapMaterial_Master()
    {
        SwapMaterial_Head();
        SwapMaterial_Body();
    }


    public void SwapMaterial_Head()
    {
        if (_materials == null || _materials.Length == 0)
        {
            Debug.LogWarning($"ScottsBackup_PlayerMaterialSwapper - No materials assigned on {name}!");
            return;
        }

        // Pick a random index
        int randomIndex = Random.Range(0, _materials.Length);

        // Assign the new material
        objRenderer_Head.material = _materials[randomIndex];
    }

    public void SwapMaterial_Body()
    {
        if (_materials == null || _materials.Length == 0)
        {
            Debug.LogWarning($"ScottsBackup_PlayerMaterialSwapper - No materials assigned on {name}!");
            return;
        }

        // Pick a random index
        int randomIndex = Random.Range(0, _materials.Length);

        // Assign the new material
        objRenderer_Body.material = _materials[randomIndex];
    }
}
