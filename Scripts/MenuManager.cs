using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JaimesUtilities.SaveLoad;
using UnityEngine.EventSystems;

// This script is not refactorized.

public class MenuManager : MonoBehaviour
{
    public enum MenuTab {
        Home,
        Options,
        About,
        Credits,
        Quit,
        COUNT
    }

    public static MenuManager instance;
    private EventSystem eventSystem;

    [SerializeField] private AudioSource audioClick;
    [SerializeField] private Transform mainMenuParent;
    [SerializeField] private Transform sidebarParent;
    [SerializeField] private Transform pagesParent;
    public MenuTab currentTab;

    [Header("Home")]
    [SerializeField] private Button homePage_StartButton;
    [SerializeField] private Button homePage_ResumeButton;
    [SerializeField] private TextMeshProUGUI homePage_Description;
    private bool homePage_StartButtonHovering;
    private bool homePage_ResumeButtonHovering;

    [Header("Settings")]
    [SerializeField] private Toggle settingsPage_Fullscreen;
    [SerializeField] private Toggle settingsPage_VSync;
    [SerializeField] private Slider settingsPage_AudioMaster;
    [SerializeField] private Slider settingsPage_AudioMusic;
    [SerializeField] private Slider settingsPage_AudioEffects;
    [SerializeField] private Slider settingsPage_Sensitivity;
    [SerializeField] private Transform keyBindingParent;
    private Dictionary<string, TMP_InputField> keyBindingInputFields = new Dictionary<string, TMP_InputField>();
    private Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();

    [Header("Quit")]
    [SerializeField] private Button quitPage_ExitButton;
    private bool quitPage_ExitButtonHovering;

    private SceneSettings sceneSettings;
    private string focussingKeyBindingInputField = "";
    private bool ingameSoundStopped;

    private void Awake() {
        instance = this;
        eventSystem = FindObjectOfType<EventSystem>();

        for (int i = 0; i < keyBindingParent.childCount; i++) {
            keyBindingInputFields.Add(keyBindingParent.GetChild(i).name, keyBindingParent.GetChild(i).GetComponentInChildren<TMP_InputField>());
            keyBindings.Add(keyBindingParent.GetChild(i).name, KeyCode.A);
        }
    }
    private void Start() {
        UpdateSettings();
        audioClick.volume = 0.4f;

        if (GameManager.SceneIndex > 0) {
            sceneSettings = WorldManager.Instance.settings.sceneSettings[GameManager.SceneIndex - 1];
        }
    }

    private void Update() {
        WorldManager.Instance.settings.audioMixer.SetFloat("VolumeInGame", (GameManager.Paused || ingameSoundStopped) ? -80 : 0);

        if (GameManager.SceneIndex > 0 && SettingsManager.GetInput("Menu", out KeyCode menuInput) && Input.GetKeyDown(menuInput)) {
            if (GameManager.Paused) {
                GameManager.ResumeGame();
                audioClick.Play();
            }
            else {
                GameManager.PauseGame();
                audioClick.Play();
            }
        }

        if (GameManager.SceneIndex > 0) {
            homePage_Description.text = "";

            for (int i = 0; i < sceneSettings.objectives.Length; i++) {
                if (WorldManager.Instance.objectiveIndex > i) homePage_Description.text += $"- <s>{sceneSettings.objectives[i].description}</s>\n\n";
                else homePage_Description.text += $"- {sceneSettings.objectives[i].description}\n\n";
            }
        }

        mainMenuParent.gameObject.SetActive(GameManager.Paused);

        for (int i = 0; i < (int)MenuTab.COUNT; i++) {
            sidebarParent.GetChild(i).Find("Highlight").gameObject.SetActive((int)currentTab == i);
            pagesParent.GetChild(i).gameObject.SetActive((int)currentTab == i);
        }

        if (GameManager.Paused) { 
            switch (currentTab) {
                case MenuTab.Home:
                    TextMeshProUGUI homePage_StartTextMesh = homePage_StartButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (homePage_StartButtonHovering) homePage_StartTextMesh.fontStyle |= FontStyles.Underline;
                    else homePage_StartTextMesh.fontStyle &= ~FontStyles.Underline;

                    TextMeshProUGUI homePage_ResumeTextMesh = homePage_ResumeButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (homePage_ResumeButtonHovering) homePage_ResumeTextMesh.fontStyle |= FontStyles.Underline;
                    else homePage_ResumeTextMesh.fontStyle &= ~FontStyles.Underline;

                    homePage_StartButton.gameObject.SetActive(GameManager.SceneIndex == 0);
                    homePage_ResumeButton.gameObject.SetActive(GameManager.SceneIndex > 0);
                    break;

                case MenuTab.Quit:
                    TextMeshProUGUI quitPage_ExitTextMesh = quitPage_ExitButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (quitPage_ExitButtonHovering) quitPage_ExitTextMesh.fontStyle |= FontStyles.Underline;
                    else quitPage_ExitTextMesh.fontStyle &= ~FontStyles.Underline;
                    break;

                case MenuTab.Options:
                    ApplySettings();
                    UpdateSettings();
                    break;
            }
        }
    }

    // Keybinding control
    private void OnGUI() {
        if (currentTab == MenuTab.Options) {
            Event e = Event.current;
            if (focussingKeyBindingInputField != "") {
                bool set = false;

                if (e.isKey && e.type == EventType.KeyDown) {
                    KeyCode k = e.keyCode;

                    keyBindingInputFields[focussingKeyBindingInputField].text = k.ToString();
                    keyBindings[focussingKeyBindingInputField] = k;
                    set = true;
                } 
                else if (e.isMouse && e.type == EventType.MouseDown) {
                    KeyCode k = 
                        e.button == 0 ? KeyCode.Mouse0 :
                        e.button == 1 ? KeyCode.Mouse1 :
                                        KeyCode.Mouse2;

                    keyBindingInputFields[focussingKeyBindingInputField].text = k.ToString();
                    keyBindings[focussingKeyBindingInputField] = k;
                    set = true;
                }

                if (set) { 
                    focussingKeyBindingInputField = "";
                    if (eventSystem != null) eventSystem.SetSelectedGameObject(null);
                }
            }
        }
    }

    // Edit settings
    public void ApplySettings() {
        SettingsManager.SetBoolean("Fullscreen", settingsPage_Fullscreen.isOn, false);
        SettingsManager.SetBoolean("VSync", settingsPage_VSync.isOn, false);
        SettingsManager.SetFloat("MouseSensitivity", settingsPage_Sensitivity.value, false);
        SettingsManager.SetAudio("Master", settingsPage_AudioMaster.value, false);
        SettingsManager.SetAudio("Music", settingsPage_AudioMusic.value, false);
        SettingsManager.SetAudio("Effects", settingsPage_AudioEffects.value, false);

        foreach (string key in keyBindingInputFields.Keys) {
            SettingsManager.SetInput(key, keyBindings[key], false);
        }
        SettingsManager.SaveSettings();
    }
    public void UpdateSettings() {
        // Audio
        if (SettingsManager.GetAudio("Master", false, out float volumeMaster)) {
            WorldManager.Instance.settings.audioMixer.SetFloat("VolumeMaster", volumeMaster);
        }
        if (SettingsManager.GetAudio("Master", true, out float volumeMasterPerc)) {
            settingsPage_AudioMaster.value = volumeMasterPerc;
        }
        if (SettingsManager.GetAudio("Music", false, out float volumeMusic)) {
            WorldManager.Instance.settings.audioMixer.SetFloat("VolumeMusic", volumeMusic);
        }
        if (SettingsManager.GetAudio("Music", true, out float volumeMusicPerc)) {
            settingsPage_AudioMusic.value = volumeMusicPerc;
        }
        if (SettingsManager.GetAudio("Effects", false, out float volumeEffects)) {
            WorldManager.Instance.settings.audioMixer.SetFloat("VolumeEffects", volumeEffects);
        }
        if (SettingsManager.GetAudio("Effects", true, out float volumeEffectsPerc)) {
            settingsPage_AudioEffects.value = volumeEffectsPerc;
        }

        // Video
        if (SettingsManager.GetBoolean("VSync", out bool vsync)) {
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            settingsPage_VSync.isOn = vsync;
        }
        if (SettingsManager.GetBoolean("Fullscreen", out bool fullScreen)) {
            Screen.fullScreen = fullScreen;
            Screen.fullScreenMode = Screen.fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            settingsPage_Fullscreen.isOn = fullScreen;
        }

        // Controls
        if (SettingsManager.GetFloat("MouseSensitivity", out float sensitivity)) settingsPage_Sensitivity.value = sensitivity;

        foreach (string key in keyBindingInputFields.Keys) {
            if (SettingsManager.GetInput(key, out KeyCode code)) {
                keyBindings[key] = code;
                keyBindingInputFields[key].text = code.ToString();
            }
        }
    }

    // Change Tab
    public void OnClickMenuTab(int index) {
        if (index == (int)currentTab) return;
        audioClick.Play();
        currentTab = (MenuTab)index;
    }

    // Home
    public void HomePage_OnClickStartGame() {
        audioClick.Play();
        WorldManager.Instance.NextLevel();
    }
    public void HomePage_OnClickResumeGame() {
        audioClick.Play();
        GameManager.ResumeGame();
    }
    public void HomePage_OnPointerStartGame(bool value) {
        homePage_StartButtonHovering = value;
    }
    public void HomePage_OnPointerResumeGame(bool value) {
        homePage_ResumeButtonHovering = value;
    }

    // Settings
    public void OnSelectKeyInputField(string key) {
        audioClick.Play();
        focussingKeyBindingInputField = key;
    }
    public void OnDeselectKeyInputField() {
        audioClick.Play();
        focussingKeyBindingInputField = "";
    }

    // Quit
    public void QuitPage_OnClickExitGame() {
        audioClick.Play();
        Application.Quit();
    }
    public void QuitPage_OnPointerStartGame(bool value) {
        quitPage_ExitButtonHovering = value;
    }

    // Overrides
    public void StopIngameSound() {
        ingameSoundStopped = true;
    }
}
