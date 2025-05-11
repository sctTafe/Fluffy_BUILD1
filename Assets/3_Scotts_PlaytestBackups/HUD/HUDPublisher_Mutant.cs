using UnityEngine;

public class HUDPublisher_Mutant : HUDPublisher
{

    [Header("Action Mapping Systems (Auto Assings)")]
    ScottsBackup_ActionMappingSystem _ActionMappingSystem;

    [Header("Resource Systems")]
    [SerializeField] ScottsBackup_ResourceMng _mutantEng_ResoruceMngs;
    public ScottsBackup_ResourceMng _MutantResMng { get { return _mutantEng_ResoruceMngs; } }


    [Header("Abiliities")]
    [SerializeField] private PlayerActionBase _inputMain;
    [SerializeField] private PlayerActionBase _inputSprint;
    [SerializeField] private PlayerActionBase _Action_Interaction1;
    [SerializeField] private PlayerActionBase _Action_Interaction2;
    [SerializeField] private PlayerActionBase _Action_Interaction3;
    [SerializeField] private PlayerActionBase _Action_Interaction4;
    public PlayerActionBase InputMain => _inputMain;
    public PlayerActionBase InputSprint => _inputSprint;
    public PlayerActionBase ActionInteraction1 => _Action_Interaction1;
    public PlayerActionBase ActionInteraction2 => _Action_Interaction2;
    public PlayerActionBase ActionInteraction3 => _Action_Interaction3;
    public PlayerActionBase ActionInteraction4 => _Action_Interaction4;

    protected override void Start()
    {
        base.Start();

        SetHudType(HUDManager.HUDType.Mutant);

        _ActionMappingSystem = GetComponent<ScottsBackup_ActionMappingSystem>();
        if (_ActionMappingSystem == null)
        {
            Debug.LogError("ActionMappingSystem is Null!");
        }
        else
        {
            Bind_ActionMappingSystem();
        }
            
        if (_MutantResMng == null)
            Debug.LogError("MutantResMng is Null!");

        HUDManager.Instance.fn_BindPublisherType(this);

    }


    private void Bind_ActionMappingSystem()
    {
        _inputMain = _ActionMappingSystem._Action_Main;
        _inputSprint = _ActionMappingSystem._Action_Sprint;
        _Action_Interaction1 = _ActionMappingSystem._Action_Interaction1;
        _Action_Interaction2 = _ActionMappingSystem._Action_Interaction2;
        _Action_Interaction3 = _ActionMappingSystem._Action_Interaction3;
        _Action_Interaction4 = _ActionMappingSystem._Action_Interaction4;
    }

}
