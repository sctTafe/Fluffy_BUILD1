using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRelayJoinCode : MonoBehaviour
{
    [SerializeField] private string _relayCode = "RELAYCODE";
    [SerializeField] private TextMeshProUGUI _relayCodeTxt;
    [SerializeField] private Button _relayCopyCodeButton;


    private void Awake()
    {
        if (_relayCopyCodeButton != null)
        {
            _relayCopyCodeButton.onClick.AddListener(CopyCodeToClipboard);
        }
    }

    private void OnDestroy()
    {
        LobbySharedJoinCode.Instance.OnRelayCodeChanged -= Handle_OnRelayCodeChnage;
    }

    private void Start()
    {
        // use network variable for clients
        _relayCode = LobbySharedJoinCode.Instance.relayJoinCode;
        UpdateLobyJoinCodeTxt();

        // bind to event encase updated
        LobbySharedJoinCode.Instance.OnRelayCodeChanged += Handle_OnRelayCodeChnage;
  
    }

    private void Handle_OnRelayCodeChnage(object sender, EventArgs e)
    {
        _relayCode = LobbySharedJoinCode.Instance.relayJoinCode;
        UpdateLobyJoinCodeTxt();
    }

    private void UpdateLobyJoinCodeTxt()
    {
        _relayCodeTxt.text = _relayCode;
    }

    private void CopyCodeToClipboard()
    {
        // Sets the 'System CopyBuffer clipboard' to the join code
        GUIUtility.systemCopyBuffer = _relayCode;
        Debug.Log($"Relay Join code '{_relayCode}' copied to clipboard!");
    }
}
