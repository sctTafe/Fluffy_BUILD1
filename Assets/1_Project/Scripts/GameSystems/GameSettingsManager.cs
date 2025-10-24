using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class GameSettingsManager : MonoBehaviour
{
    [Header("Audio Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider ambientSlider;

    [Header("Graphics")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public TMP_Dropdown qualityDropdown;

    [Header("Gameplay")]
    public Slider mouseSensitivitySlider;

    private Resolution[] resolutions;

    // FMOD VCAs
    private VCA masterVCA;
    private VCA musicVCA;
    private VCA sfxVCA;
    private VCA ambientVCA;

    void Start()
    {
        masterVCA = RuntimeManager.GetVCA("vca:/Master");
        musicVCA = RuntimeManager.GetVCA("vca:/Music");
        sfxVCA = RuntimeManager.GetVCA("vca:/SFX");
        ambientVCA = RuntimeManager.GetVCA("vca:/Ambient");

        SetupResolutions();
        LoadSettings();
        RegisterUIEvents();
    }

    // Register UI events for easy use in the Editor
    private void RegisterUIEvents()
    {
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener((index) => { SetResolution(index); SaveSettings(); });
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener((isFullscreen) => { SetFullscreen(isFullscreen); SaveSettings(); });
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener((index) => { SetQuality(index); SaveSettings(); });
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener((value) => { SetMasterVolume(value); SaveSettings(); });
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener((value) => { SetMusicVolume(value); SaveSettings(); });
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener((value) => { SetSFXVolume(value); SaveSettings(); });
        if (ambientSlider != null)
            ambientSlider.onValueChanged.AddListener((value) => { SetAmbientVolume(value); SaveSettings(); });
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener((value) => { SetMouseSensitivity(value); SaveSettings(); });
    }

    //void SetupResolutions()
    //{
    //    resolutions = Screen.resolutions;

    //    if (resolutionDropdown == null) return;
    //    resolutionDropdown.ClearOptions();

    //    List<string> options = new List<string>();
    //    int currentIndex = 0;

    //    for (int i = 0; i < resolutions.Length; i++)
    //    {
    //        string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "Hz";
    //        options.Add(option);

    //        if (resolutions[i].width == Screen.currentResolution.width &&
    //            resolutions[i].height == Screen.currentResolution.height &&
    //            resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
    //        {
    //            currentIndex = i;
    //        }
    //    }

    //    resolutionDropdown.AddOptions(options);
    //    resolutionDropdown.value = currentIndex;
    //    resolutionDropdown.RefreshShownValue();
    //}

    void SetupResolutions()
    {
        resolutions = FilterResolutions(Screen.resolutions);
        if (resolutionDropdown == null) return;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private Resolution[] FilterResolutions(Resolution[] allResolutions)
    {
        List<Resolution> filteredResolutions = new List<Resolution>();

        var res_all = Screen.resolutions;

        // Define allowed resolutions (width, height)
        var allowedResolutions = new List<(int width, int height)>
        {
            (1280, 720),   // 720p
            (1600, 900),   // 1600x900
            (1920, 1080),  // 1080p
            (2560, 1440)   // 1440p (2K)
        };

        // First pass: try to find resolutions at common refresh rates (59-61Hz range)
        foreach (Resolution res in allResolutions)
        {
            foreach (var allowed in allowedResolutions)
            {
                if (res.width == allowed.width &&
                    res.height == allowed.height &&
                    res.refreshRate >= 45 && res.refreshRate <= 70)
                {
                    filteredResolutions.Add(res);
                    break;
                }
            }
        }

        // Second pass: if no 60Hz resolutions found, get the highest refresh rate for each allowed resolution
        if (filteredResolutions.Count == 0)
        {
            Debug.Log("No 60Hz resolutions found. Using highest available refresh rate for each resolution.");

            foreach (var allowed in allowedResolutions)
            {
                Resolution? highestRefreshRes = null;

                foreach (Resolution res in allResolutions)
                {
                    if (res.width == allowed.width && res.height == allowed.height)
                    {
                        if (highestRefreshRes == null || res.refreshRate > highestRefreshRes.Value.refreshRate)
                        {
                            highestRefreshRes = res;
                        }
                    }
                }

                if (highestRefreshRes != null)
                {
                    filteredResolutions.Add(highestRefreshRes.Value);
                }
            }
        }

        // Final fallback: if still no resolutions matched, use all available
        if (filteredResolutions.Count == 0)
        {
            Debug.LogWarning("No matching resolutions found. Using all available resolutions.");
            return allResolutions;
        }

        return filteredResolutions.ToArray();
    }



    public void SetResolution(int index)
    {
        if (resolutions == null || resolutions.Length == 0) return;
        if (index < 0 || index >= resolutions.Length) return;
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRate);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    public void SetMasterVolume(float value)
    {
        if (masterVCA.isValid())
            masterVCA.setVolume(value);
    }

    public void SetMusicVolume(float value)
    {
        if (musicVCA.isValid())
            musicVCA.setVolume(value);
    }

    public void SetSFXVolume(float value)
    {
        if (sfxVCA.isValid())
            sfxVCA.setVolume(value);
    }

    public void SetAmbientVolume(float value)
    {
        if (ambientVCA.isValid())
            ambientVCA.setVolume(value);
    }

    public void SetMouseSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }

    public void SaveSettings()
    {
        if (resolutionDropdown != null)
            PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);
        if (fullscreenToggle != null)
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        if (qualityDropdown != null)
            PlayerPrefs.SetInt("Quality", qualityDropdown.value);
        if (masterSlider != null)
            PlayerPrefs.SetFloat("MasterVol", masterSlider.value);
        if (musicSlider != null)
            PlayerPrefs.SetFloat("MusicVol", musicSlider.value);
        if (sfxSlider != null)
            PlayerPrefs.SetFloat("SFXVol", sfxSlider.value);
        if (ambientSlider != null)
            PlayerPrefs.SetFloat("AmbientVol", ambientSlider.value);
        if (mouseSensitivitySlider != null)
            PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        if (resolutionDropdown != null && PlayerPrefs.HasKey("Resolution"))
        {
            int resIndex = PlayerPrefs.GetInt("Resolution");
            Debug.Log("Loaded Resolution Index: " + resIndex);
            resolutionDropdown.value = resIndex;
            SetResolution(resIndex);
        }
        if (fullscreenToggle != null)
        {
            bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            Debug.Log("Loaded Fullscreen: " + fullscreen);
            fullscreenToggle.isOn = fullscreen;
            SetFullscreen(fullscreenToggle.isOn);
        }
        int qualityIndex = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        Debug.Log("Loaded Quality Index: " + qualityIndex);
        if (qualityDropdown != null)
        {
            qualityDropdown.value = qualityIndex;
        }
        SetQuality(qualityIndex);

        float master = PlayerPrefs.GetFloat("MasterVol", 0.75f);
        float music = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVol", 0.75f);
        float ambient = PlayerPrefs.GetFloat("AmbientVol", 0.75f);
        Debug.Log($"Loaded Volumes - Master: {master}, Music: {music}, SFX: {sfx}, Ambient: {ambient}");
        if (masterSlider != null) masterSlider.value = master;
        if (musicSlider != null) musicSlider.value = music;
        if (sfxSlider != null) sfxSlider.value = sfx;
        if (ambientSlider != null) ambientSlider.value = ambient;
        SetMasterVolume(master);
        SetMusicVolume(music);
        SetSFXVolume(sfx);
        SetAmbientVolume(ambient);

        float mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 0.75f);
        Debug.Log("Loaded Mouse Sensitivity: " + mouseSensitivity);
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = mouseSensitivity;
    }
}