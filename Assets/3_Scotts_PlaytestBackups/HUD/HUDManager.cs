using UnityEngine;

/// <summary>
/// Author:
/// Scott Barley 07/05/25
/// DOSE:
///     Contactable by the player game object, works out which HUD to display and passes through the local data reference
/// SUMMARY:
///     Middle Man Script for connecting player HUD Data to the HUD Publisher Scripts
/// </summary>

public class HUDManager : Singleton<HUDManager>
{
    public enum HUDType
    {
        error,
        Fluffy,
        Ghost,
        Mutant
    }
    private const bool ISDEBUGGING = false;

    // HUD Canvas GO's
    [SerializeField] GameObject _fluffyCanvasGO;
    [SerializeField] GameObject _mutantCanvasGO;
    [SerializeField] GameObject _ghostCanvasGO;

    // Role related HUD Mngs 
    HUD_Fluffy _hUDPublisher_Fluffy;
    HUD_Mutant _hUDPublisher_Mutant;
    HUD_Ghost _hUDPublisher_Ghost;

    // 
    private void Start()
    {
        //Ensure All Canvas are disabled at the start of the game
        DisableAllCanvases();
        GetHudPublisherComponents();
    }

    public void fn_BindPublisherType(HUDPublisher hUDPublisher)
    {
        if (ISDEBUGGING) Debug.Log("HUDPublisher: fn_BindPublisherType Called");
        Handle_BindPublisherType(hUDPublisher);
    }

    private void Handle_BindPublisherType(HUDPublisher hUDPublisher)
    {
        HUDType ht = hUDPublisher.HUDType;

        if (ht == HUDType.error)
        {
            Debug.LogError("HUDManager; Somthing Fucked Up!");
            return;
        }

        DisableAllCanvases();

        switch (ht)
        {
            case HUDType.Fluffy:
                _fluffyCanvasGO.SetActive(true);
                _hUDPublisher_Fluffy.fn_Bind(hUDPublisher);
                if (ISDEBUGGING) Debug.Log("HUDPublisher: Fluffy HUD Activated");
                break;
            case HUDType.Mutant:
                _mutantCanvasGO.SetActive(true);
                _hUDPublisher_Mutant.fn_Bind(hUDPublisher);
                if (ISDEBUGGING) Debug.Log("HUDPublisher: Mutant HUD Activated");
                break;
            case HUDType.Ghost:
                _ghostCanvasGO.SetActive(true);
                _hUDPublisher_Ghost.fn_Bind(hUDPublisher);
                if (ISDEBUGGING) Debug.Log("HUDPublisher: Ghost HUD Activated");
                break;
        }
    }

    private void DisableAllCanvases()
    {
        // Disable All
        _fluffyCanvasGO.SetActive(false);
        _mutantCanvasGO.SetActive(false);
        _ghostCanvasGO.SetActive(false);
    }

    private void GetHudPublisherComponents()
    {      
        _hUDPublisher_Fluffy = _fluffyCanvasGO.GetComponent<HUD_Fluffy>();
        _hUDPublisher_Mutant = _mutantCanvasGO.GetComponent <HUD_Mutant>();
        _hUDPublisher_Ghost = _ghostCanvasGO.GetComponent<HUD_Ghost>();
    }

}




