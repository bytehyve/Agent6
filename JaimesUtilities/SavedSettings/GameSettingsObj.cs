using UnityEngine;

namespace JaimesUtilities.SaveLoad { 
    
    [CreateAssetMenu(fileName = "Game Settings", menuName = "JaimesUtilities/GameSettings")]
    public class GameSettingsObj : ScriptableObject
    {
        public GameSettings settings;
        [Tooltip("Amount of UpdateTicks before calling the OnValuesUpdated event")]
        public ushort updateTick;
    }
}