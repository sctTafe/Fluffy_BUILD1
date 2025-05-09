using UnityEngine;

public class HUDPublisher_Mutant : HUDPublisher
{
    [Header("Resource Systems")]
    [SerializeField] ScottsBackup_ResourceMng _mutantEng_ResoruceMngs;
    public ScottsBackup_ResourceMng _MutantResMng { get { return _mutantEng_ResoruceMngs; } }
    protected override void Start()
    {
        base.Start();

        SetHudType(HUDManager.HUDType.Mutant);


        if (_MutantResMng == null)
            Debug.LogError("MutantResMng is Null!");

        HUDManager.Instance.fn_BindPublisherType(this);

    }

    protected override void Update()
    {
        base.Update();
    }


}
