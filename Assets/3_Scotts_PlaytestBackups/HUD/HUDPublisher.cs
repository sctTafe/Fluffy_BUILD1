using UnityEngine;

/// <summary>
/// 
/// Sends the Data/Refs To the HUD
/// 
/// </summary>
public abstract class HUDPublisher : MonoBehaviour
{
    private const bool ISDEBUGGING = false;

    [Header("Abiliities Mapping (Auto Assings)")]
    [SerializeField] protected PlayerActionBase _inputMain;
    [SerializeField] protected PlayerActionBase _inputSprint;
    [SerializeField] protected PlayerActionBase _inputJump;
    [SerializeField] protected PlayerActionBase _Action_Interaction1;
    [SerializeField] protected PlayerActionBase _Action_Interaction2;
    [SerializeField] protected PlayerActionBase _Action_Interaction3;
    [SerializeField] protected PlayerActionBase _Action_Interaction4;
    public PlayerActionBase InputMain => _inputMain;
    public PlayerActionBase InputSprint => _inputSprint;
    public PlayerActionBase InputJump => _inputJump;
    public PlayerActionBase ActionInteraction1 => _Action_Interaction1;
    public PlayerActionBase ActionInteraction2 => _Action_Interaction2;
    public PlayerActionBase ActionInteraction3 => _Action_Interaction3;
    public PlayerActionBase ActionInteraction4 => _Action_Interaction4;

    [Header("Action Mapping Systems (Auto Assings)")]
    protected ScottsBackup_ActionMappingSystem _ActionMappingSystem;

    public HUDManager.HUDType HUDType { get { return _type; } }
    protected HUDManager.HUDType _type;

    protected virtual void Start()
    {
        if (ISDEBUGGING) Debug.Log("HUDPublisher Base Called");

        _ActionMappingSystem = GetComponent<ScottsBackup_ActionMappingSystem>();
        if (_ActionMappingSystem == null)
        {
            Debug.LogError("ActionMappingSystem is Null!");
        }
        else
        {
            Bind_ActionMappingSystem();
        }
    }

    protected virtual void Update()
    {       
    }

    protected void SetHudType(HUDManager.HUDType type)
    {
        this._type = type;
    }

    private void Bind_ActionMappingSystem()
    {
        _inputMain = _ActionMappingSystem._Action_Main;
        _inputSprint = _ActionMappingSystem._Action_Sprint;
        _inputJump = _ActionMappingSystem._Action_Jump;
        _Action_Interaction1 = _ActionMappingSystem._Action_Interaction1;
        _Action_Interaction2 = _ActionMappingSystem._Action_Interaction2;
        _Action_Interaction3 = _ActionMappingSystem._Action_Interaction3;
        _Action_Interaction4 = _ActionMappingSystem._Action_Interaction4;
    }
}
