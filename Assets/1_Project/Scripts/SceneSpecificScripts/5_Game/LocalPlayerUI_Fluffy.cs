using UnityEngine;
using UnityEngine.UI;

public class LocalPlayerUI_Fluffy : Singleton<LocalPlayerUI_Fluffy>
{
    FluffyPlayerDataManager_Local _LocalPlayerData;

    [SerializeField] private GameObject staminaBar;

    float _stamina, _maxStamina = 1000;
    float _lerpSpeed = 5f;

    void Start()
    {
        _stamina = _maxStamina;
        staminaBar = GameObject.FindWithTag("player_stamina_bar");
    }

    void Update()
    {
        if (_LocalPlayerData == null)
            return;

        if (_stamina > _maxStamina) _stamina = _maxStamina;
        StaminaBarFiller();
    }

    public void fn_BindLocalPlayerData(FluffyPlayerDataManager_Local _Local)
    {
        _LocalPlayerData = _Local;
    }

    public void fn_SetGolemStamina_Pct(float stamina_pct)
    {
        _stamina = stamina_pct * _maxStamina;
    }

    void StaminaBarFiller()
    {
        float targetFill = _LocalPlayerData.fn_GetStaminaPercent();
       // if (staminaBar != null) 
       //    // staminaBar.fillAmount = Mathf.Lerp(staminaBar.fillAmount, targetFill, _lerpSpeed * Time.deltaTime);
       // else { Debug.LogWarning("This kept returning null in other scenes, so I put a null check <3 Izaac"); }
    }

}
