using UnityEngine;

public class HUDPublisher_Mutant : HUDPublisher
{
    private const bool ISDEBUGGING = false;

    [Header("Resource Systems")]
    [SerializeField] ScottsBackup_ResourceMng _mutantEng_ResoruceMngs;
    public ScottsBackup_ResourceMng _MutantResMng { get { return _mutantEng_ResoruceMngs; } }

    private void Awake()
    {
        if (ISDEBUGGING) Debug.Log("HUDPublisher_Mutant: Awake Called");
    }


    protected override void Start()
    {
        // Disable Self If Not Owner
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }

        if (ISDEBUGGING) Debug.Log("HUDPublisher_Mutant: Start Called");

        SetHudType(HUDManager.HUDType.Mutant);

        base.Start();

        if (ISDEBUGGING)
        {
            Debug.Log($"HUDPublisher_Mutant: _ActionMappingSystem: {_ActionMappingSystem.name}");
            DebugOut();
        }

        if (_MutantResMng == null)
            Debug.LogError("MutantResMng is Null!");

        HUDManager.Instance.fn_BindPublisherType(this);

        
    }



    private void DebugOut()
    {
        if (_inputMain != null)
            Debug.Log("HUDPublisher_Mutant: _inputMain: Found");

        if (_inputSprint != null)
            Debug.Log("HUDPublisher_Mutant: _inputSprint: Found");

        if (_inputJump != null)
            Debug.Log("HUDPublisher_Mutant: _inputJump: Found");

        if (_Action_Interaction1 != null)
            Debug.Log("HUDPublisher_Mutant: _Action_Interaction1: Found");

        if (_Action_Interaction2 != null)
            Debug.Log("HUDPublisher_Mutant: _Action_Interaction2: Found");

        if (_Action_Interaction3 != null)
            Debug.Log("HUDPublisher_Mutant: _Action_Interaction3: Found");

        if (_Action_Interaction4 != null)
            Debug.Log("HUDPublisher_Mutant: _Action_Interaction4: Found");
    }
}
