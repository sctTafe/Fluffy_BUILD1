using UnityEngine;

public class HUDPublisher_Ghost : HUDPublisher
{
    protected override void Start()
    {
        base.Start();

        SetHudType(HUDManager.HUDType.Ghost);
        HUDManager.Instance.fn_BindPublisherType(this);

    }

    protected override void Update()
    {
        base.Update();
    }
}
