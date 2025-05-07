using UnityEngine;

public class HUDPublisher_Fluffy : HUDPublisher
{
    protected override void Start()
    {
        base.Start();
        SetHudType(HUDManager.HUDType.Fluffy);
        HUDManager.Instance.fn_BindPublisherType(this);
    }

    protected override void Update()
    {
        base.Update();
    }
}
