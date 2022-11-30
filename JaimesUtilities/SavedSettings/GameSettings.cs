using UnityEngine;

namespace JaimesUtilities.SaveLoad { 
    
    [System.Serializable]
    public class GameSettings 
    {
        public Setting_Boolean[] settings_boolean;
        public Setting_Integer[] settings_integer;
        public Setting_Float[] settings_float;
        public Setting_String[] settings_string;
        public Setting_Audio[] settings_audio;
        public Setting_Input[] settings_input;

        public GameSettings() {
            settings_boolean = new Setting_Boolean[0];
            settings_integer = new Setting_Integer[0];
            settings_float = new Setting_Float[0];
            settings_string = new Setting_String[0];
            settings_audio = new Setting_Audio[0];
            settings_input = new Setting_Input[0];
        }

        [System.Serializable]
        public struct Setting_Boolean {
            public string name;
            public bool value;
        }
        [System.Serializable]
        public struct Setting_Integer {
            public string name;
            public int value;
        }
        [System.Serializable]
        public struct Setting_Float {
            public string name;
            public float value;
        }
        [System.Serializable]
        public struct Setting_String {
            public string name;
            public string value;
        }
        [System.Serializable]
        public struct Setting_Audio {
            public string name;
            [Range(0, 1f)]
            public float value;
        }
        [System.Serializable]
        public struct Setting_Input {
            public string name;
            public KeyCode value;
        }
    }
}