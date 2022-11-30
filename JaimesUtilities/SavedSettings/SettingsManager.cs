using UnityEngine;
using System.IO;

namespace JaimesUtilities.SaveLoad
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] private string dataPath = "settings";
        [SerializeField] private GameSettingsObj defaultSettingsObj;

        public delegate void OnChangeAction();
        public static event OnChangeAction OnValuesUpdated;

        static ushort updateTick;
        static bool valuesUpdated;
        static ushort valuesUpdateTick;

        public static GameSettings settings;
        public static string DataPath;

        private void Awake() {
            DataPath = dataPath;
            if (defaultSettingsObj != null) {
                settings = JsonUtility.FromJson<GameSettings>(JsonUtility.ToJson(defaultSettingsObj.settings));
            }
            updateTick = defaultSettingsObj.updateTick;
            LoadSettings();
        }
        private void Update() {
            if (valuesUpdated) {
                valuesUpdateTick -= 1;

                if (valuesUpdateTick < 0) { 
                    OnValuesUpdated.Invoke();
                    valuesUpdated = false;
                }
            }
        }
        private void OnApplicationQuit() {
            SaveSettings();
        }

        public static void SaveSettings(bool overwrite = true) {
            if (settings == null) {
                settings = new GameSettings();
            }

            string settingsJson = JsonUtility.ToJson(settings);
            string filePath = $"{Application.dataPath}/{DataPath}.json";
            
            if (File.Exists(filePath)) {
                if (!overwrite) return;
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, settingsJson);
        }
        public static bool LoadSettings() {
            string filePath = $"{Application.dataPath}/{DataPath}.json";
            
            if (File.Exists(filePath)) {
                try {
                    settings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(filePath));
                    valuesUpdated = true;
                    return true;
                }
                catch {
                    Debug.Log($"Error: Couldn't load settings file!");
                    SaveSettings();
                }
            }
            else SaveSettings();
            return false;
        }

        public static float GetAudioVolumeFromPercentage(float percentage) {
            if (percentage < 0.01f) percentage = 0.01f;
            percentage = -Mathf.Pow(percentage - 1f, 2f) + 1f;
            return Mathf.Clamp(Mathf.Log(percentage) * 30f + 5f, -80f, 15f);
        }

        // Get
        public static bool GetBoolean(string name, out bool result) {
            for (int i = 0; i < settings.settings_boolean.Length; i++) {
                if (settings.settings_boolean[i].name.Equals(name)) {
                    result = settings.settings_boolean[i].value;
                    return true;
                }
            }
            result = false;
            return false;
        }
        public static bool GetInteger(string name, out int result) {
            for (int i = 0; i < settings.settings_integer.Length; i++) {
                if (settings.settings_integer[i].name.Equals(name)) {
                    result = settings.settings_integer[i].value;
                    return true;
                }
            }
            result = 0;
            return false;
        }
        public static bool GetFloat(string name, out float result) {
            for (int i = 0; i < settings.settings_float.Length; i++) {
                if (settings.settings_float[i].name.Equals(name)) {
                    result = settings.settings_float[i].value;
                    return true;
                }
            }
            result = 0;
            return false;
        }
        public static bool GetString(string name, out string result) {
            for (int i = 0; i < settings.settings_string.Length; i++) {
                if (settings.settings_string[i].name.Equals(name)) {
                    result = settings.settings_string[i].value;
                    return true;
                }
            }
            result = "";
            return false;
        }
        public static bool GetAudio(string name, bool asPercentage, out float result) {
            for (int i = 0; i < settings.settings_audio.Length; i++) {
                if (settings.settings_audio[i].name.Equals(name)) {
                    result = asPercentage ? settings.settings_audio[i].value : GetAudioVolumeFromPercentage(settings.settings_audio[i].value.Clamp());
                    return true;
                }
            }
            result = asPercentage ? 0 : -80;
            return false;
        }
        public static bool GetInput(string name, out KeyCode result) {
            for (int i = 0; i < settings.settings_input.Length; i++) {
                if (settings.settings_input[i].name.Equals(name)) {
                    result = settings.settings_input[i].value;
                    return true;
                }
            }
            result = KeyCode.None;
            return false;
        }

        // Set
        public static bool SetBoolean(string name, bool value, bool autoSave = true) {
            for (int i = 0; i < settings.settings_boolean.Length; i++) {
                if (settings.settings_boolean[i].name.Equals(name)) {
                    settings.settings_boolean[i].value = value;
                    valuesUpdated = true;
                    valuesUpdateTick = updateTick;
                    if (autoSave) SaveSettings();
                    return true;
                }
            }
            return false;
        }
        public static bool SetInteger(string name, int value, bool autoSave = true) {
            for (int i = 0; i < settings.settings_integer.Length; i++) {
                if (settings.settings_integer[i].name.Equals(name)) {
                    settings.settings_integer[i].value = value;
                    valuesUpdated = true;
                    valuesUpdateTick = updateTick;
                    if (autoSave) SaveSettings();
                    return true;
                }
            }
            return false;
        }
        public static bool SetFloat(string name, float value, bool autoSave = true) {
            for (int i = 0; i < settings.settings_float.Length; i++) {
                if (settings.settings_float[i].name.Equals(name)) {
                    settings.settings_float[i].value = value;
                    valuesUpdated = true;
                    valuesUpdateTick = updateTick;
                    if (autoSave) SaveSettings();
                    return true;
                }
            }
            return false;
        }
        public static bool SetString(string name, string value, bool autoSave = true) {
            for (int i = 0; i < settings.settings_string.Length; i++) {
                if (settings.settings_string[i].name.Equals(name)) {
                    settings.settings_string[i].value = value;
                    valuesUpdated = true;
                    valuesUpdateTick = updateTick;
                    if (autoSave) SaveSettings();
                    return true;
                }
            }
            return false;
        }
        public static bool SetAudio(string name, float valuePercentage, bool autoSave = true) {
            for (int i = 0; i < settings.settings_audio.Length; i++) {
                if (settings.settings_audio[i].name.Equals(name)) {
                    settings.settings_audio[i].value = valuePercentage.Clamp();
                    valuesUpdated = true;
                    valuesUpdateTick = updateTick;
                    if (autoSave) SaveSettings();
                    return true;
                }
            }
            return false;
        }
        public static bool SetInput(string name, KeyCode value, bool autoSave = true) {
            for (int i = 0; i < settings.settings_input.Length; i++) {
                if (settings.settings_input[i].name.Equals(name)) {
                    settings.settings_input[i].value = value;
                    valuesUpdated = true;
                    valuesUpdateTick = updateTick;
                    if (autoSave) SaveSettings();
                    return true;
                }
            }
            return false;
        }
    }
}
