using UnityEngine;

public class HUD : MonoBehaviour
{
    private const bool ISDEBUGGING = false;

    [Header("Action Buttons - Manual Assignment Needed")]
    [SerializeField] protected GameObject _ActionUIElement_Main;
    [SerializeField] protected GameObject _ActionUIElement_Sprint;
    [SerializeField] protected GameObject _ActionUIElement_Jump;
    [SerializeField] protected GameObject _ActionUIElement_1;
    [SerializeField] protected GameObject _ActionUIElement_2;
    [SerializeField] protected GameObject _ActionUIElement_3;
    [SerializeField] protected GameObject _ActionUIElement_4;

    protected HUDAbilityMng _hUDAbilityMng_Main;
    protected HUDAbilityMng _hUDAbilityMng_Sprint;
    protected HUDAbilityMng _hUDAbilityMng_Jump;
    protected HUDAbilityMng _hUDAbilityMng_1;
    protected HUDAbilityMng _hUDAbilityMng_2;
    protected HUDAbilityMng _hUDAbilityMng_3;
    protected HUDAbilityMng _hUDAbilityMng_4;

    protected HUDPublisher _bound_hUDPublisher;


    
    protected virtual void Start()
    {
        if (ISDEBUGGING) Debug.Log("HUD: Start Called!");
        //DisableAllActionButtons();
    }

    protected virtual void Update()
    {
    }

    private void OnDisable()
    {
        Unbind();
    }
    private void OnDestroy()
    {
        Unbind();
    }

    public virtual void fn_Bind(HUDPublisher hUDPublisher)
    {
        if (ISDEBUGGING) Debug.Log("HUD: fn_Bind Called!");
        Unbind();
        DisableAllActionButtons();
        _bound_hUDPublisher = hUDPublisher;
        TryBindActionButtons();
    }


    protected virtual void Unbind()
    {
        _bound_hUDPublisher = null;
    }

    protected void DisableAllActionButtons()
    {
        if (ISDEBUGGING) Debug.Log("HUD: DisableAllActionButtons Called!");

        if (_ActionUIElement_Main != null)
            _ActionUIElement_Main.SetActive(false);

        if (_ActionUIElement_Sprint != null)
            _ActionUIElement_Sprint.SetActive(false);

        if (_ActionUIElement_Jump != null)
            _ActionUIElement_Jump.SetActive(false);

        if (_ActionUIElement_1 != null)
            _ActionUIElement_1.SetActive(false);

        if (_ActionUIElement_2 != null)
            _ActionUIElement_2.SetActive(false);

        if (_ActionUIElement_3 != null)
            _ActionUIElement_3.SetActive(false);

        if (_ActionUIElement_4 != null)
            _ActionUIElement_4.SetActive(false);
    }

    protected void TryBindActionButtons()
    {
        if (ISDEBUGGING) Debug.Log("HUD: fn_Bind TryBindActionButtons!");
        TryBindButton(_bound_hUDPublisher.InputMain, _ActionUIElement_Main, _hUDAbilityMng_Main);
        TryBindButton(_bound_hUDPublisher.InputSprint, _ActionUIElement_Sprint, _hUDAbilityMng_Sprint);
        TryBindButton(_bound_hUDPublisher.InputJump, _ActionUIElement_Jump, _hUDAbilityMng_Jump);
        TryBindButton(_bound_hUDPublisher.ActionInteraction1, _ActionUIElement_1, _hUDAbilityMng_1);
        TryBindButton(_bound_hUDPublisher.ActionInteraction2, _ActionUIElement_2, _hUDAbilityMng_2);
        TryBindButton(_bound_hUDPublisher.ActionInteraction3, _ActionUIElement_3, _hUDAbilityMng_3);
        TryBindButton(_bound_hUDPublisher.ActionInteraction4, _ActionUIElement_4, _hUDAbilityMng_4);

        void TryBindButton(PlayerActionBase action, GameObject uiElement, HUDAbilityMng abilityMng)
        {
            abilityMng = uiElement.GetComponent<HUDAbilityMng>();

            var binder = action as IHudAbilityBinder;
            if (binder != null)
            {
                //if (ISDEBUGGING) Debug.Log($"HUD: TryBindActionButtons IHudAbilityBinder Found for {uiElement.name}!");
                uiElement.SetActive(true);
                abilityMng.fn_Bind(binder);
            }
            else
            {
                //if (ISDEBUGGING) Debug.Log($"HUD: TryBindActionButtons IHudAbilityBinder NOT Found for {uiElement.name}!");
                uiElement.SetActive(false);
            }
        }
    }


}
