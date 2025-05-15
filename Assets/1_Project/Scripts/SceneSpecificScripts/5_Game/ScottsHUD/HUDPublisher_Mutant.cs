using UnityEngine;

public class HUDPublisher_Mutant : HUDPublisher
{
    protected override void Start()
    {
        base.Start();

        SetHudType(HUDManager.HUDType.Mutant);
        HUDManager.Instance.fn_BindPublisherType(this);

    }

    protected override void Update()
    {
        base.Update();
    }


}
