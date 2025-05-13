using UnityEngine;

public class HUDPublisher_Fluffy : HUDPublisher
{

    // References to the systems that need to be displayed
    [Header("Resource Systems")]
    [SerializeField] ScottsBackup_ResourceMng _stamina_ResoruceMngs;
    [SerializeField] ScottsBackup_ResourceMng _health_ResoruceMngs;

    public ScottsBackup_ResourceMng _StaminaResMng { get { return _stamina_ResoruceMngs; } }
    public ScottsBackup_ResourceMng _HealthResMng { get { return _health_ResoruceMngs; } }


    protected override void Start()
    {      
        SetHudType(HUDManager.HUDType.Fluffy);

        base.Start();

        if (_StaminaResMng == null)
            Debug.LogError("StaminaResMng is Null!");
        if (_HealthResMng == null)
            Debug.LogError("HealthResMng is Null!");

        HUDManager.Instance.fn_BindPublisherType(this);
    }

}
