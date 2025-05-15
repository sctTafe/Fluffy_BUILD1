using UnityEngine;
using TMPro;

public class GameVersionNumberUI : MonoBehaviour
{
    public TextMeshProUGUI versionText;

    void Start()
    {
        versionText.text = "Game Version: " + Application.version;
    }
}
