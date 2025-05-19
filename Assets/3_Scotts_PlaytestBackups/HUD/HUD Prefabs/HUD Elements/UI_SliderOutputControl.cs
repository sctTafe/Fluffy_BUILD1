using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: 
///     Scott Barley  07/05/2025
/// IS: 
///     Slider Fill amount controler with Lerp Mode
/// </summary>
public class UI_SliderOutputControl : MonoBehaviour
{
    public Image _fillBar_Img;
    public float _startFillPct = 0.5f;
    public float _fillLerpCoeff = 0.5f;

    public bool _isFillBarFullOnStart;

    private float _currentTargetValue, _maxValue = 1000;
    private float _barUiLerpSpeed;
    private float _targetPct;
    private bool _isLerpModeActive = false;



    private void Start()
    {
        if (_isFillBarFullOnStart)
            fn_SetFillPct_NoLerp(1f);
    }

    private void Update()
    {

        LerpBarFiller();
    }

    void LerpBarFiller()
    {
        if (_isLerpModeActive)
        {
            _barUiLerpSpeed = _fillLerpCoeff * Time.deltaTime;
            _fillBar_Img.fillAmount = Mathf.Lerp(_fillBar_Img.fillAmount, _currentTargetValue / _maxValue, _barUiLerpSpeed);


            float currentFill = _fillBar_Img.fillAmount;
            float dif = Mathf.Sqrt((currentFill - _targetPct) * (currentFill - _targetPct));

            if (dif < 0.001)
                _isLerpModeActive = false;
        }
    }

    public void fn_SetFillPct_Lerp(float fill_pct)
    {
        _currentTargetValue = _maxValue * sanitize_MinMaxValues(fill_pct);
        _targetPct = sanitize_MinMaxValues(fill_pct);
        _isLerpModeActive = true;
    }
    public void fn_SetFillPct_NoLerp(float fill_pct)
    {
        _fillBar_Img.fillAmount = sanitize_MinMaxValues(fill_pct);
        _isLerpModeActive = false;
    }
    private float sanitize_MinMaxValues(float fill_pct)
    {
        if (fill_pct < 0.0f)
            return 0.0f;
        if (fill_pct > 1.0f)
            return 1.0f;
        return fill_pct;
    }
}
