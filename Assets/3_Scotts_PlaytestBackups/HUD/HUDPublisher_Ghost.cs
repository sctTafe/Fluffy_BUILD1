using UnityEngine;

public class HUDPublisher_Ghost : HUDPublisher
{
    [Header("Resource Systems")]
    [SerializeField] ScottsBackup_ResourceMng _ghostEnergy_ResoruceMngs;
    public ScottsBackup_ResourceMng _GhostResMng { get { return _ghostEnergy_ResoruceMngs; } }

    protected override void Start()
    {
        base.Start();

        SetHudType(HUDManager.HUDType.Ghost);

        if (_GhostResMng == null)
            Debug.LogError("GhostResMng is Null!");

        HUDManager.Instance.fn_BindPublisherType(this);

    }

    protected override void Update()
    {
        base.Update();
    }
}
