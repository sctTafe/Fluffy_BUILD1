using UnityEngine;

/// <summary>
/// 
/// Sends the Data/Refs To the HUD
/// 
/// </summary>
public abstract class HUDPublisher : MonoBehaviour
{
    public HUDManager.HUDType HUDType { get { return _type; } }
    protected HUDManager.HUDType _type;

    protected virtual void Start()
    {     
    }

    protected virtual void Update()
    {       
    }

    protected void SetHudType(HUDManager.HUDType type)
    {
        this._type = type;
    }
}
